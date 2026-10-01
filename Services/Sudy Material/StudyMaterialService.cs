// Services/StudyMaterialService.cs — full updated file
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Upload;
using Tutor_Manager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Tutor_Manager.Services
{
    public class StudyMaterialService : IStudyMaterialService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IStudyMaterialFileStorageService _fileStorage;

        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };
        private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20MB
        private const int MaxFileCount = 10;

        public StudyMaterialService(Tutor_ManagerDatabaseContext context, IStudyMaterialFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<StudyMaterialResult> CreateAsync(StudyMaterialUploadViewModel model, int tutorUserId)
        {
            var validationError = ValidateFiles(model.Files, existingKeptCount: 0);
            if (validationError != null)
                return StudyMaterialResult.Failure(validationError);

            var material = new StudyMaterial
            {
                SubjectId = model.SubjectId,
                TutorUserId = tutorUserId,
                Title = model.Title,
                Description = model.Description,
                TopicChapterLabel = model.TopicChapterLabel,
                UploadedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.StudyMaterials.Add(material);
            await _context.SaveChangesAsync();

            var referenceNumber = material.Id.ToString();
            foreach (var file in model.Files)
            {
                var storageKey = await _fileStorage.SaveFileAsync(file, referenceNumber);

                _context.StudyMaterialFiles.Add(new StudyMaterialFile
                {
                    StudyMaterialId = material.Id,
                    FileName = file.FileName,
                    StoragePath = storageKey,
                    FileType = GetFileType(file.FileName),
                    UploadedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(material.Id);
        }

        public async Task<StudyMaterial?> GetByIdAsync(int id, int tutorUserId)
        {
            return await _context.StudyMaterials
                .Include(m => m.Files)
                .Include(m => m.Subject)
                .FirstOrDefaultAsync(m => m.Id == id && m.TutorUserId == tutorUserId && m.IsActive);
        }

        public async Task<StudyMaterialResult> UpdateAsync(StudyMaterialEditViewModel model, int tutorUserId)
        {
            var material = await _context.StudyMaterials
                .Include(m => m.Files)
                .FirstOrDefaultAsync(m => m.Id == model.Id && m.TutorUserId == tutorUserId && m.IsActive);

            if (material == null)
                return StudyMaterialResult.Failure("Study material not found.");

            var keptExistingCount = material.Files.Count(f => !model.FileIdsToRemove.Contains(f.Id));

            var validationError = ValidateFiles(model.NewFiles, existingKeptCount: keptExistingCount);
            if (validationError != null)
                return StudyMaterialResult.Failure(validationError);

            if (keptExistingCount + model.NewFiles.Count == 0)
                return StudyMaterialResult.Failure("A study material must have at least one file.");

            material.SubjectId = model.SubjectId;
            material.Title = model.Title;
            material.Description = model.Description;
            material.TopicChapterLabel = model.TopicChapterLabel;

            // Remove files the tutor unchecked
            var filesToRemove = material.Files.Where(f => model.FileIdsToRemove.Contains(f.Id)).ToList();
            foreach (var file in filesToRemove)
            {
                _fileStorage.DeleteFile(file.StoragePath);
                _context.StudyMaterialFiles.Remove(file);
            }

            // Add newly selected files
            var referenceNumber = material.Id.ToString();
            foreach (var file in model.NewFiles)
            {
                var storageKey = await _fileStorage.SaveFileAsync(file, referenceNumber);

                _context.StudyMaterialFiles.Add(new StudyMaterialFile
                {
                    StudyMaterialId = material.Id,
                    FileName = file.FileName,
                    StoragePath = storageKey,
                    FileType = GetFileType(file.FileName),
                    UploadedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(material.Id);
        }

        public async Task<StudyMaterialResult> DeleteAsync(int id, int tutorUserId)
        {
            var material = await _context.StudyMaterials
                .FirstOrDefaultAsync(m => m.Id == id && m.TutorUserId == tutorUserId && m.IsActive);

            if (material == null)
                return StudyMaterialResult.Failure("Study material not found.");

            material.IsActive = false;
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(material.Id);
        }

        public async Task<StudyMaterialResult> BulkDeleteAsync(List<int> ids, int tutorUserId)
        {
            var materials = await _context.StudyMaterials
                .Where(m => ids.Contains(m.Id) && m.TutorUserId == tutorUserId && m.IsActive)
                .ToListAsync();

            foreach (var material in materials)
            {
                material.IsActive = false;
            }

            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(materials.Count);
        }

        private string ValidateFiles(List<IFormFile> newFiles, int existingKeptCount)
        {
            if (existingKeptCount + newFiles.Count > MaxFileCount)
                return $"You can have a maximum of {MaxFileCount} files total (currently would be {existingKeptCount + newFiles.Count}).";

            foreach (var file in newFiles)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                    return $"File '{file.FileName}' is not an allowed type. Only PDF, Word, and PowerPoint files are supported.";

                if (file.Length > MaxFileSizeBytes)
                    return $"File '{file.FileName}' exceeds the 20MB size limit.";
            }

            return null;
        }

        private MaterialFileType GetFileType(string fileName)
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