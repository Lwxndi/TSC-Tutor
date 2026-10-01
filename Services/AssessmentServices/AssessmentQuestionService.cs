// Services/AssessmentServices/AssessmentQuestionService.cs
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.AssessmentViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public class AssessmentQuestionService : IAssessmentQuestionService
    {
        private readonly Tutor_ManagerDatabaseContext _context;

        public AssessmentQuestionService(Tutor_ManagerDatabaseContext context)
        {
            _context = context;
        }

        public async Task<Assessment?> GetForReviewAsync(int assessmentId, int tutorUserId)
        {
            return await _context.Assessments
                .Include(a => a.Questions.OrderBy(q => q.DisplayOrder))
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TutorUserId == tutorUserId && a.IsActive);
        }

        public async Task<AssessmentQuestion?> GetQuestionAsync(int questionId, int assessmentId, int tutorUserId)
        {
            var exists = await _context.Assessments
                .AnyAsync(a => a.Id == assessmentId && a.TutorUserId == tutorUserId && a.IsActive);
            if (!exists)
                return null;

            return await _context.AssessmentQuestions
                .FirstOrDefaultAsync(q => q.Id == questionId && q.AssessmentId == assessmentId);
        }

        public async Task<StudyMaterialResult> UpdateQuestionAsync(AssessmentQuestionEditViewModel model, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .FirstOrDefaultAsync(a => a.Id == model.AssessmentId && a.TutorUserId == tutorUserId && a.IsActive);
            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.Generated)
                return StudyMaterialResult.Failure("This assessment is not currently open for question review.");

            var question = await _context.AssessmentQuestions
                .FirstOrDefaultAsync(q => q.Id == model.Id && q.AssessmentId == model.AssessmentId);
            if (question == null)
                return StudyMaterialResult.Failure("Question not found.");

            question.QuestionText = model.QuestionText;
            question.Marks = model.Marks;
            question.MarkingGuidance = model.MarkingGuidance;

            await _context.SaveChangesAsync();
            return StudyMaterialResult.Success(question.Id);
        }

        public async Task<StudyMaterialResult> AddQuestionAsync(AssessmentQuestionEditViewModel model, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == model.AssessmentId && a.TutorUserId == tutorUserId && a.IsActive);
            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.Generated)
                return StudyMaterialResult.Failure("This assessment is not currently open for question review.");

            var question = new AssessmentQuestion
            {
                AssessmentId = assessment.Id,
                QuestionText = model.QuestionText,
                Marks = model.Marks,
                MarkingGuidance = model.MarkingGuidance,
                DisplayOrder = assessment.Questions.Any() ? assessment.Questions.Max(q => q.DisplayOrder) + 1 : 1
            };

            _context.AssessmentQuestions.Add(question);
            await _context.SaveChangesAsync();
            return StudyMaterialResult.Success(question.Id);
        }

        public async Task<StudyMaterialResult> DeleteQuestionAsync(int questionId, int assessmentId, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TutorUserId == tutorUserId && a.IsActive);
            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.Generated)
                return StudyMaterialResult.Failure("This assessment is not currently open for question review.");

            var question = await _context.AssessmentQuestions
                .FirstOrDefaultAsync(q => q.Id == questionId && q.AssessmentId == assessmentId);
            if (question == null)
                return StudyMaterialResult.Failure("Question not found.");

            _context.AssessmentQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(assessmentId);
        }

        public async Task<StudyMaterialResult> ConfirmAsync(int assessmentId, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TutorUserId == tutorUserId && a.IsActive);
            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.Generated)
                return StudyMaterialResult.Failure("This assessment is not in a state that can be confirmed.");

            if (!assessment.Questions.Any())
                return StudyMaterialResult.Failure("An assessment must have at least one question before it can be confirmed.");

            assessment.Status = AssessmentGenerationStatus.Confirmed;
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(assessment.Id);
        }

        public async Task<StudyMaterialResult> UnconfirmAsync(int assessmentId, int tutorUserId)
        {
            var assessment = await _context.Assessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TutorUserId == tutorUserId && a.IsActive);
            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.Confirmed)
                return StudyMaterialResult.Failure("This assessment is not currently confirmed.");

            // Fixed: was checking DueDate (a display-only field) as the lock. OpenAt is the
            // real "has this gone live" gate now that Step 10 constraints exist — matches
            // the same lock UpdateAsync uses.
            if (assessment.OpenAt <= DateTime.UtcNow)
                return StudyMaterialResult.Failure("This assessment has already opened and can no longer be un-confirmed.");

            assessment.Status = AssessmentGenerationStatus.Generated;
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(assessment.Id);
        }
    }
}