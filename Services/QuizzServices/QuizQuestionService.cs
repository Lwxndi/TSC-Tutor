using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public class QuizQuestionService : IQuizQuestionService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IQuizImageStorage _images;

        public QuizQuestionService(Tutor_ManagerDatabaseContext context, IQuizImageStorage images)
        {
            _context = context;
            _images = images;
        }

        public async Task<Quiz?> GetQuizForReviewAsync(int quizId, int tutorUserId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions.OrderBy(qq => qq.DisplayOrder))
                    .ThenInclude(qq => qq.Options.OrderBy(o => o.DisplayOrder))
                .FirstOrDefaultAsync(q => q.Id == quizId && q.TutorUserId == tutorUserId && q.IsActive);
        }

        public async Task<QuizQuestion?> GetQuestionAsync(int questionId, int quizId, int tutorUserId)
        {
            var quizExists = await _context.Quizzes
                .AnyAsync(q => q.Id == quizId && q.TutorUserId == tutorUserId && q.IsActive);
            if (!quizExists)
                return null;

            return await _context.QuizQuestions
                .Include(q => q.Options.OrderBy(o => o.DisplayOrder))
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuizId == quizId);
        }

        public async Task<StudyMaterialResult> UpdateQuestionAsync(QuizQuestionEditViewModel model, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == model.QuizId && q.TutorUserId == tutorUserId && q.IsActive);
            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            //if (quiz.Status != QuizGenerationStatus.Generated)
            //    return StudyMaterialResult.Failure("This quiz is not currently open for question review.");

            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == model.Id && q.QuizId == model.QuizId);
            if (question == null)
                return StudyMaterialResult.Failure("Question not found.");

            var validationError = ValidateQuestionContent(model.QuestionType, model.Options, model.CorrectBoolAnswer, model.MarkingGuidance);
            if (validationError != null)
                return StudyMaterialResult.Failure(validationError);

            var imgError = await ApplyImageAsync(question, model);
            if (imgError != null)
                return StudyMaterialResult.Failure(imgError);

            question.QuestionType = model.QuestionType;
            question.QuestionText = model.QuestionText;
            question.Marks = model.Marks;

            // Fields not relevant to the (possibly new) type are always cleared and rebuilt
            // from the submitted model — this runs whether or not the type actually changed,
            // since it's simpler than tracking "did type change" separately. The warning shown
            // client-side (Views/Quiz/EditQuestion.cshtml) is what actually protects the tutor
            // from losing data by accident; this method trusts that the warning was heeded.
            _context.QuizQuestionOptions.RemoveRange(question.Options);
            question.Options.Clear();
            question.CorrectBoolAnswer = null;
            question.MarkingGuidance = null;

            ApplyTypeSpecificFields(question, model);

            await _context.SaveChangesAsync();
            return StudyMaterialResult.Success(question.Id);
        }

        public async Task<StudyMaterialResult> AddQuestionAsync(QuizQuestionEditViewModel model, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == model.QuizId && q.TutorUserId == tutorUserId && q.IsActive);
            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            //if (quiz.Status != QuizGenerationStatus.Generated)
            //    return StudyMaterialResult.Failure("This quiz is not currently open for question review.");

            var validationError = ValidateQuestionContent(model.QuestionType, model.Options, model.CorrectBoolAnswer, model.MarkingGuidance);
            if (validationError != null)
                return StudyMaterialResult.Failure(validationError);

            var question = new QuizQuestion
            {
                QuizId = quiz.Id,
                QuestionType = model.QuestionType,
                QuestionText = model.QuestionText,
                Marks = model.Marks,
                DisplayOrder = quiz.Questions.Any() ? quiz.Questions.Max(q => q.DisplayOrder) + 1 : 1
            };

            var imgError = await ApplyImageAsync(question, model);
            if (imgError != null)
                return StudyMaterialResult.Failure(imgError);

            ApplyTypeSpecificFields(question, model);

            _context.QuizQuestions.Add(question);
            await _context.SaveChangesAsync();
            return StudyMaterialResult.Success(question.Id);
        }

        public async Task<StudyMaterialResult> DeleteQuestionAsync(int questionId, int quizId, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId && q.TutorUserId == tutorUserId && q.IsActive);
            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            //if (quiz.Status != QuizGenerationStatus.Generated)
            //    return StudyMaterialResult.Failure("This quiz is not currently open for question review.");

            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuizId == quizId);
            if (question == null)
                return StudyMaterialResult.Failure("Question not found.");

            // Hard delete, not soft — QuizQuestion has no IsActive field, and no attempt
            // data exists yet (Step 5 not built) that would need this question preserved.
            _context.QuizQuestionOptions.RemoveRange(question.Options);
            _context.QuizQuestions.Remove(question);
            await _context.SaveChangesAsync();

            _images.Delete(question.ImagePath);

            return StudyMaterialResult.Success(quizId);
        }

        public async Task<StudyMaterialResult> ConfirmAsync(int quizId, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.TutorUserId == tutorUserId && q.IsActive);
            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            if (quiz.Status != QuizGenerationStatus.Generated)
                return StudyMaterialResult.Failure("This quiz is not in a state that can be confirmed.");

            if (!quiz.Questions.Any())
                return StudyMaterialResult.Failure("A quiz must have at least one question before it can be confirmed.");

            quiz.Status = QuizGenerationStatus.Confirmed;
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(quiz.Id);
        }

        private static void ApplyTypeSpecificFields(QuizQuestion question, QuizQuestionEditViewModel model)
        {
            switch (model.QuestionType)
            {
                case QuizQuestionType.MultipleChoiceSingle:
                case QuizQuestionType.MultipleChoiceMultiple:
                    var order = 1;
                    foreach (var opt in model.Options)
                    {
                        question.Options.Add(new QuizQuestionOption
                        {
                            OptionText = opt.OptionText,
                            IsCorrect = opt.IsCorrect,
                            DisplayOrder = order++
                        });
                    }
                    break;

                case QuizQuestionType.TrueFalse:
                    question.CorrectBoolAnswer = model.CorrectBoolAnswer;
                    break;

                case QuizQuestionType.ShortAnswer:
                case QuizQuestionType.Essay:
                case QuizQuestionType.NumericEquation:
                    question.MarkingGuidance = model.MarkingGuidance;
                    break;
            }
        }

        private static string? ValidateQuestionContent(
            QuizQuestionType type,
            List<QuizQuestionOptionEditViewModel> options,
            bool? correctBoolAnswer,
            string? markingGuidance)
        {
            switch (type)
            {
                case QuizQuestionType.MultipleChoiceSingle:
                    if (options == null || options.Count < 2)
                        return "At least two options are required.";
                    if (options.Count(o => o.IsCorrect) != 1)
                        return "Exactly one option must be marked correct for a single-answer multiple choice question.";
                    break;

                case QuizQuestionType.MultipleChoiceMultiple:
                    if (options == null || options.Count < 2)
                        return "At least two options are required.";
                    if (!options.Any(o => o.IsCorrect))
                        return "At least one option must be marked correct.";
                    break;

                case QuizQuestionType.TrueFalse:
                    if (correctBoolAnswer == null)
                        return "A correct answer (True or False) must be selected.";
                    break;

                case QuizQuestionType.ShortAnswer:
                case QuizQuestionType.Essay:
                case QuizQuestionType.NumericEquation:
                    if (string.IsNullOrWhiteSpace(markingGuidance))
                        return "Marking guidance is required for this question type.";
                    break;
            }

            return null;
        }

        public async Task<StudyMaterialResult> UnconfirmAsync(int quizId, int tutorUserId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId && q.TutorUserId == tutorUserId && q.IsActive);
            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            if (quiz.Status != QuizGenerationStatus.Confirmed)
                return StudyMaterialResult.Failure("This quiz is not currently confirmed.");

            // Same lock as UpdateAsync/Edit(GET) in QuizService — once a quiz has opened,
            // it can no longer be pulled back into an editable state.
            if (quiz.OpenAt <= DateTime.UtcNow)
                return StudyMaterialResult.Failure("This quiz has already opened and can no longer be un-confirmed.");

            quiz.Status = QuizGenerationStatus.Generated;
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(quiz.Id);
        }


        private async Task<string?> ApplyImageAsync(QuizQuestion question, QuizQuestionEditViewModel model)
        {
            if (model.ImageFile is { Length: > 0 })
            {
                if (!_images.IsAllowed(model.ImageFile, out var error)) return error;
                var old = question.ImagePath;
                question.ImagePath = await _images.SaveAsync(model.ImageFile);
                _images.Delete(old);
            }
            else if (model.RemoveImage && question.ImagePath != null)
            {
                _images.Delete(question.ImagePath);
                question.ImagePath = null;
            }
            return null;
        }
    }
}