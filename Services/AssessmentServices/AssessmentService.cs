using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Upload;
using Tutor_Manager.ViewModels.AssessmentViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public class AssessmentService : IAssessmentService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IAssessmentFileStorageService _fileStorage;

        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };
        private const long MaxFileSizeBytes = 20 * 1024 * 1024;

        public AssessmentService(Tutor_ManagerDatabaseContext context, IAssessmentFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<StudyMaterialResult> CreateAsync(AssessmentCreateViewModel model, int tutorUserId)
        {
            var fileError = ValidateFile(model.QuestionPaperFile) ?? ValidateFile(model.MarkingGuidelineFile);
            if (fileError != null)
                return StudyMaterialResult.Failure(fileError);

            var constraintError = await ValidateConstraintsAsync(model.SubjectId, model.OpenAt, model.CloseAt,
                model.AudienceType, model.OfferingId, model.SelectedLearnerUserIds, tutorUserId);
            if (constraintError != null)
                return StudyMaterialResult.Failure(constraintError);

            var referenceNumber = Guid.NewGuid().ToString();

            string qpKey;
            string mgKey;
            try
            {
                qpKey = await _fileStorage.SaveFileAsync(model.QuestionPaperFile, referenceNumber);
                mgKey = await _fileStorage.SaveFileAsync(model.MarkingGuidelineFile, referenceNumber);
            }
            catch (Exception)
            {
                CleanupReferenceFolder(referenceNumber);
                return StudyMaterialResult.Failure("Failed to save the uploaded files. Please try again.");
            }

            var assessment = new Assessment
            {
                SubjectId = model.SubjectId,
                TutorUserId = tutorUserId,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate.ToDateTime(TimeOnly.MinValue),
                DeclaredTotalMarks = model.DeclaredTotalMarks,
                Status = AssessmentGenerationStatus.NotGenerated,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                OpenAt = model.OpenAt,
                CloseAt = model.CloseAt,
                MaxAttempts = model.MaxAttempts,
                AudienceType = model.AudienceType,
                OfferingId = model.AudienceType == QuizzTypes.SpecificOffering ? model.OfferingId : null,
                Files = new List<AssessmentFile>
                {
                    new AssessmentFile
                    {
                        FileName = model.QuestionPaperFile.FileName,
                        StoragePath = qpKey,
                        FileType = GetFileType(model.QuestionPaperFile.FileName),
                        Role = AssessmentFileRole.QuestionPaper,
                        UploadedAt = DateTime.UtcNow
                    },
                    new AssessmentFile
                    {
                        FileName = model.MarkingGuidelineFile.FileName,
                        StoragePath = mgKey,
                        FileType = GetFileType(model.MarkingGuidelineFile.FileName),
                        Role = AssessmentFileRole.MarkingGuideline,
                        UploadedAt = DateTime.UtcNow
                    }
                }
            };

            _context.Assessments.Add(assessment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                CleanupReferenceFolder(referenceNumber);
                return StudyMaterialResult.Failure("Failed to save the assessment. Please try again.");
            }

            if (model.AudienceType == QuizzTypes.SpecificLearners && model.SelectedLearnerUserIds.Any())
            {
                foreach (var learnerId in model.SelectedLearnerUserIds)
                {
                    _context.AssessmentLearners.Add(new AssessmentLearner
                    {
                        AssessmentId = assessment.Id,
                        LearnerUserId = learnerId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return StudyMaterialResult.Success(assessment.Id);
        }

        public async Task<Assessment?> GetByIdAsync(int id, int tutorUserId)
        {
            return await _context.Assessments
                .Include(a => a.Files)
                .Include(a => a.Subject)
                .Include(a => a.AudienceLearners)
                .FirstOrDefaultAsync(a => a.Id == id && a.TutorUserId == tutorUserId && a.IsActive);
        }

        public async Task<StudyMaterialResult> UpdateAsync(AssessmentEditViewModel model, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Files)
                .Include(a => a.AudienceLearners)
                .FirstOrDefaultAsync(a => a.Id == model.Id && a.TutorUserId == tutorUserId && a.IsActive);

            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.NotGenerated)
                return StudyMaterialResult.Failure("This assessment has already been generated and can no longer be edited.");

            // Real lock now — replaces the old DueDate stand-in.
            if (assessment.OpenAt <= DateTime.UtcNow)
                return StudyMaterialResult.Failure("This assessment has already opened and can no longer be edited.");

            var constraintError = await ValidateConstraintsAsync(model.SubjectId, model.OpenAt, model.CloseAt,
                model.AudienceType, model.OfferingId, model.SelectedLearnerUserIds, tutorUserId);
            if (constraintError != null)
                return StudyMaterialResult.Failure(constraintError);

            if (model.NewQuestionPaperFile != null)
            {
                var error = ValidateFile(model.NewQuestionPaperFile);
                if (error != null)
                    return StudyMaterialResult.Failure(error);
            }

            if (model.NewMarkingGuidelineFile != null)
            {
                var error = ValidateFile(model.NewMarkingGuidelineFile);
                if (error != null)
                    return StudyMaterialResult.Failure(error);
            }

            var referenceNumber = assessment.Id.ToString();
            var newlySavedKeys = new List<string>();

            try
            {
                if (model.NewQuestionPaperFile != null)
                {
                    var oldFile = assessment.Files.First(f => f.Role == AssessmentFileRole.QuestionPaper);
                    var newKey = await _fileStorage.SaveFileAsync(model.NewQuestionPaperFile, referenceNumber);
                    newlySavedKeys.Add(newKey);

                    oldFile.FileName = model.NewQuestionPaperFile.FileName;
                    var previousPath = oldFile.StoragePath;
                    oldFile.StoragePath = newKey;
                    oldFile.FileType = GetFileType(model.NewQuestionPaperFile.FileName);
                    oldFile.UploadedAt = DateTime.UtcNow;
                    _fileStorage.DeleteFile(previousPath);
                }

                if (model.NewMarkingGuidelineFile != null)
                {
                    var oldFile = assessment.Files.First(f => f.Role == AssessmentFileRole.MarkingGuideline);
                    var newKey = await _fileStorage.SaveFileAsync(model.NewMarkingGuidelineFile, referenceNumber);
                    newlySavedKeys.Add(newKey);

                    oldFile.FileName = model.NewMarkingGuidelineFile.FileName;
                    var previousPath = oldFile.StoragePath;
                    oldFile.StoragePath = newKey;
                    oldFile.FileType = GetFileType(model.NewMarkingGuidelineFile.FileName);
                    oldFile.UploadedAt = DateTime.UtcNow;
                    _fileStorage.DeleteFile(previousPath);
                }
            }
            catch (Exception)
            {
                foreach (var key in newlySavedKeys)
                    _fileStorage.DeleteFile(key);
                return StudyMaterialResult.Failure("Failed to save the uploaded files. Please try again.");
            }

            assessment.SubjectId = model.SubjectId;
            assessment.Title = model.Title;
            assessment.Description = model.Description;
            assessment.DueDate = model.DueDate.ToDateTime(TimeOnly.MinValue);
            assessment.DeclaredTotalMarks = model.DeclaredTotalMarks;
            assessment.OpenAt = model.OpenAt;
            assessment.CloseAt = model.CloseAt;
            assessment.MaxAttempts = model.MaxAttempts;
            assessment.AudienceType = model.AudienceType;
            assessment.OfferingId = model.AudienceType == QuizzTypes.SpecificOffering ? model.OfferingId : null;

            // Additive only — same rule as Quiz's LearnerQuizz handling.
            if (model.AudienceType == QuizzTypes.SpecificLearners && model.SelectedLearnerUserIds.Any())
            {
                var alreadyAssignedIds = assessment.AudienceLearners.Select(al => al.LearnerUserId).ToHashSet();
                var newLearnerIds = model.SelectedLearnerUserIds.Where(id => !alreadyAssignedIds.Contains(id));

                foreach (var learnerId in newLearnerIds)
                {
                    _context.AssessmentLearners.Add(new AssessmentLearner
                    {
                        AssessmentId = assessment.Id,
                        LearnerUserId = learnerId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(assessment.Id);
        }

        public async Task<StudyMaterialResult> DeleteAsync(int id, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .FirstOrDefaultAsync(a => a.Id == id && a.TutorUserId == tutorUserId && a.IsActive);

            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            assessment.IsActive = false;
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(assessment.Id);
        }

        public async Task<StudyMaterialResult> BulkDeleteAsync(List<int> ids, int tutorUserId)
        {
            var assessments = await _context.Assessments
                .Where(a => ids.Contains(a.Id) && a.TutorUserId == tutorUserId && a.IsActive)
                .ToListAsync();

            foreach (var assessment in assessments)
                assessment.IsActive = false;

            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(assessments.Count);
        }

        private async Task<string?> ValidateConstraintsAsync(
            int subjectId,
            DateTime openAt,
            DateTime closeAt,
            QuizzTypes audienceType,
            int? offeringId,
            List<int> selectedLearnerUserIds,
            int tutorUserId)
        {
            var ownsSubject = await _context.TutorSubjects
                .AnyAsync(ts => ts.TutorUserId == tutorUserId && ts.SubjectId == subjectId);
            if (!ownsSubject)
                return "Selected subject was not found or does not belong to you.";

            if (closeAt <= openAt)
                return "Close date/time must be after the open date/time.";

            if (openAt < DateTime.UtcNow.AddMinutes(-5))
                return "Open date/time cannot be in the past.";

            if (audienceType == QuizzTypes.SpecificOffering)
            {
                if (offeringId == null)
                    return "An offering must be selected for this audience type.";

                var offeringExists = await _context.Offerings
                    .AnyAsync(o => o.OfferingId == offeringId && o.TutorUserId == tutorUserId && o.IsActive);
                if (!offeringExists)
                    return "Selected offering was not found or does not belong to you.";
            }

            if (audienceType == QuizzTypes.SpecificLearners && !selectedLearnerUserIds.Any())
                return "At least one learner must be selected for this audience type.";

            return null;
        }

        private string? ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Both the question paper and marking guideline are required.";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return $"File '{file.FileName}' is not an allowed type. Only PDF, Word, and PowerPoint files are supported.";

            if (file.Length > MaxFileSizeBytes)
                return $"File '{file.FileName}' exceeds the 20MB size limit.";

            return null;
        }

        private void CleanupReferenceFolder(string referenceNumber)
        {
            try
            {
                var folderPath = _fileStorage.GetFullPath(referenceNumber);
                var parentFolder = Path.GetDirectoryName(folderPath);
                if (parentFolder != null && Directory.Exists(parentFolder))
                    Directory.Delete(parentFolder, recursive: true);
            }
            catch { }
        }

        private static MaterialFileType GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => MaterialFileType.Pdf,
                ".doc" or ".docx" => MaterialFileType.Word,
                ".ppt" or ".pptx" => MaterialFileType.PowerPoint,
                _ => throw new InvalidOperationException("Unsupported file type reached storage layer.")
            };
        }
    }
}