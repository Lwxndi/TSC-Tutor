using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public class QuizService : IQuizService
    {
        private readonly Tutor_ManagerDatabaseContext _context;

        public QuizService(Tutor_ManagerDatabaseContext context)
        {
            _context = context;
        }

        public async Task<StudyMaterialResult> CreateAsync(QuizCreateViewModel model, int tutorUserId)
        {
            var validationError = await ValidateAsync(model.StudyMaterialId, model.OpenAt, model.CloseAt,
                model.AudienceType, model.OfferingId, model.SelectedLearnerUserIds, tutorUserId,
                model.MultipleChoiceSingleCount, model.MultipleChoiceMultipleCount, model.TrueFalseCount,
                model.ShortAnswerCount, model.EssayCount, model.NumericEquationCount);
            if (validationError != null)
                return StudyMaterialResult.Failure(validationError);

            var material = await _context.StudyMaterials
                .FirstOrDefaultAsync(m => m.Id == model.StudyMaterialId && m.TutorUserId == tutorUserId && m.IsActive);

            if (material == null)
                return StudyMaterialResult.Failure("Study material not found.");

            var quiz = new Quiz
            {
                StudyMaterialId = model.StudyMaterialId,
                TutorUserId = tutorUserId,
                Title = model.Title,
                OpenAt = model.OpenAt,
                CloseAt = model.CloseAt,
                DurationMinutes = model.DurationMinutes,
                MaxAttempts = model.MaxAttempts,
                AudienceType = model.AudienceType,
                OfferingId = model.AudienceType == QuizzTypes.SpecificOffering ? model.OfferingId : null,
                DifficultyLevel = model.DifficultyLevel,
                MultipleChoiceSingleCount = model.MultipleChoiceSingleCount,
                MultipleChoiceMultipleCount = model.MultipleChoiceMultipleCount,
                TrueFalseCount = model.TrueFalseCount,
                ShortAnswerCount = model.ShortAnswerCount,
                EssayCount = model.EssayCount,
                NumericEquationCount = model.NumericEquationCount,
                Status = QuizGenerationStatus.NotGenerated,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            if (model.AudienceType == QuizzTypes.SpecificLearners && model.SelectedLearnerUserIds.Any())
            {
                foreach (var learnerId in model.SelectedLearnerUserIds)
                {
                    _context.LearnerQuizzes.Add(new LearnerQuizz
                    {
                        QuizId = quiz.Id,
                        LearnerUserId = learnerId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return StudyMaterialResult.Success(quiz.Id);
        }

        public async Task<Quiz?> GetByIdAsync(int id, int tutorUserId)
        {
            return await _context.Quizzes
                .Include(q => q.StudyMaterial)
                .Include(q => q.AudienceLearners)
                .FirstOrDefaultAsync(q => q.Id == id && q.TutorUserId == tutorUserId && q.IsActive);
        }

        public async Task<StudyMaterialResult> UpdateAsync(QuizEditViewModel model, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.AudienceLearners)
                .FirstOrDefaultAsync(q => q.Id == model.Id && q.TutorUserId == tutorUserId && q.IsActive);

            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            if (quiz.OpenAt <= DateTime.UtcNow)
                return StudyMaterialResult.Failure("This quiz has already opened and can no longer be edited.");

            if (quiz.Status != QuizGenerationStatus.NotGenerated)
                return StudyMaterialResult.Failure("Questions have already been generated for this quiz and its specs can no longer be changed.");

            var validationError = await ValidateAsync(model.StudyMaterialId, model.OpenAt, model.CloseAt,
                model.AudienceType, model.OfferingId, model.SelectedLearnerUserIds, tutorUserId,
                model.MultipleChoiceSingleCount, model.MultipleChoiceMultipleCount, model.TrueFalseCount,
                model.ShortAnswerCount, model.EssayCount, model.NumericEquationCount);
            if (validationError != null)
                return StudyMaterialResult.Failure(validationError);

            var material = await _context.StudyMaterials
                .FirstOrDefaultAsync(m => m.Id == model.StudyMaterialId && m.TutorUserId == tutorUserId && m.IsActive);

            if (material == null)
                return StudyMaterialResult.Failure("Study material not found.");

            quiz.StudyMaterialId = model.StudyMaterialId;
            quiz.Title = model.Title;
            quiz.OpenAt = model.OpenAt;
            quiz.CloseAt = model.CloseAt;
            quiz.DurationMinutes = model.DurationMinutes;
            quiz.MaxAttempts = model.MaxAttempts;
            quiz.AudienceType = model.AudienceType;
            quiz.OfferingId = model.AudienceType == QuizzTypes.SpecificOffering ? model.OfferingId : null;
            quiz.DifficultyLevel = model.DifficultyLevel;
            quiz.MultipleChoiceSingleCount = model.MultipleChoiceSingleCount;
            quiz.MultipleChoiceMultipleCount = model.MultipleChoiceMultipleCount;
            quiz.TrueFalseCount = model.TrueFalseCount;
            quiz.ShortAnswerCount = model.ShortAnswerCount;
            quiz.EssayCount = model.EssayCount;
            quiz.NumericEquationCount = model.NumericEquationCount;

            if (model.AudienceType == QuizzTypes.SpecificLearners && model.SelectedLearnerUserIds.Any())
            {
                var alreadyAssignedIds = quiz.AudienceLearners.Select(al => al.LearnerUserId).ToHashSet();
                var newLearnerIds = model.SelectedLearnerUserIds.Where(id => !alreadyAssignedIds.Contains(id));

                foreach (var learnerId in newLearnerIds)
                {
                    _context.LearnerQuizzes.Add(new LearnerQuizz
                    {
                        QuizId = quiz.Id,
                        LearnerUserId = learnerId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(quiz.Id);
        }

        public async Task<StudyMaterialResult> DeleteAsync(int id, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.TutorUserId == tutorUserId && q.IsActive);

            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            quiz.IsActive = false;
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(quiz.Id);
        }

        public async Task<StudyMaterialResult> BulkDeleteAsync(List<int> ids, int tutorUserId)
        {
            var quizzes = await _context.Quizzes
                .Where(q => ids.Contains(q.Id) && q.TutorUserId == tutorUserId && q.IsActive)
                .ToListAsync();

            foreach (var quiz in quizzes)
            {
                quiz.IsActive = false;
            }

            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(quizzes.Count);
        }

        private async Task<string?> ValidateAsync(
            int studyMaterialId,
            DateTime openAt,
            DateTime closeAt,
            QuizzTypes audienceType,
            int? offeringId,
            List<int> selectedLearnerUserIds,
            int tutorUserId,
            int mcSingleCount,
            int mcMultipleCount,
            int trueFalseCount,
            int shortAnswerCount,
            int essayCount,
            int numericEquationCount)
        {
            if (closeAt <= openAt)
                return "Close date/time must be after the open date/time.";

            if (openAt < DateTime.UtcNow.AddMinutes(-5))
                return "Open date/time cannot be in the past.";

            var materialExists = await _context.StudyMaterials
                .AnyAsync(m => m.Id == studyMaterialId && m.TutorUserId == tutorUserId && m.IsActive);
            if (!materialExists)
                return "Selected study material was not found or does not belong to you.";

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

            var totalQuestions = mcSingleCount + mcMultipleCount + trueFalseCount
                + shortAnswerCount + essayCount + numericEquationCount;
            if (totalQuestions < 1)
                return "At least one question must be requested across the question type counts.";

            return null;
        }
    }
}