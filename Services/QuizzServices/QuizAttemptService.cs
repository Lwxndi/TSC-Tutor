using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Services.QuizzServices
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IQuizAttemptMarkingService _markingService;

        public QuizAttemptService(Tutor_ManagerDatabaseContext context, IQuizAttemptMarkingService markingService)
        {
            _context = context;
            _markingService = markingService;
        }

        public async Task<List<AvailableQuizViewModel>> GetAvailableQuizzesAsync(int learnerUserId)
        {
            var now = DateTime.UtcNow;

            var candidateQuizzes = await _context.Quizzes
                .Include(q => q.StudyMaterial)
                .Include(q => q.AudienceLearners)
                .Where(q => q.IsActive
                    && q.Status == QuizGenerationStatus.Confirmed
                    && q.OpenAt <= now
                    && q.CloseAt >= now)
                .ToListAsync();

            var result = new List<AvailableQuizViewModel>();

            foreach (var quiz in candidateQuizzes)
            {
                if (!await IsLearnerAllowedAsync(quiz, learnerUserId))
                    continue;

                var submittedCount = await _context.QuizAttempts
                    .CountAsync(a => a.QuizId == quiz.Id && a.LearnerUserId == learnerUserId && a.Status == QuizAttemptStatus.Submitted);

                var inProgress = await _context.QuizAttempts
                    .FirstOrDefaultAsync(a => a.QuizId == quiz.Id && a.LearnerUserId == learnerUserId && a.Status == QuizAttemptStatus.InProgress);

                result.Add(new AvailableQuizViewModel
                {
                    QuizId = quiz.Id,
                    Title = quiz.Title,
                    StudyMaterialTitle = quiz.StudyMaterial.Title,
                    OpenAt = quiz.OpenAt,
                    CloseAt = quiz.CloseAt,
                    DurationMinutes = quiz.DurationMinutes,
                    MaxAttempts = quiz.MaxAttempts,
                    SubmittedAttemptCount = submittedCount,
                    HasInProgressAttempt = inProgress != null,
                    InProgressAttemptId = inProgress?.Id
                });
            }

            return result;
        }

        public async Task<StudyMaterialResult> StartOrResumeAttemptAsync(int quizId, int learnerUserId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.StudyMaterial)
                .Include(q => q.AudienceLearners)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.IsActive);

            if (quiz == null)
                return StudyMaterialResult.Failure("Quiz not found.");

            if (quiz.Status != QuizGenerationStatus.Confirmed)
                return StudyMaterialResult.Failure("This quiz is not currently available.");

            var now = DateTime.UtcNow;
            if (now < quiz.OpenAt || now > quiz.CloseAt)
                return StudyMaterialResult.Failure("This quiz is not currently open.");

            if (!await IsLearnerAllowedAsync(quiz, learnerUserId))
                return StudyMaterialResult.Failure("You do not have access to this quiz.");

            // Resolve any existing InProgress attempt first — resume if still within time,
            // or reclassify as Abandoned (not Submitted) if its window has passed. This is
            // what makes an abandoned attempt NOT count against MaxAttempts (Decision Log).
            var existing = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.LearnerUserId == learnerUserId && a.Status == QuizAttemptStatus.InProgress);

            if (existing != null)
            {
                var deadline = existing.StartedAt.AddMinutes(quiz.DurationMinutes);
                if (now <= deadline)
                    return StudyMaterialResult.Success(existing.Id); // resume

                existing.Status = QuizAttemptStatus.Abandoned;
                await _context.SaveChangesAsync();
            }

            var submittedCount = await _context.QuizAttempts
                .CountAsync(a => a.QuizId == quizId && a.LearnerUserId == learnerUserId && a.Status == QuizAttemptStatus.Submitted);

            if (submittedCount >= quiz.MaxAttempts)
                return StudyMaterialResult.Failure("You have no attempts remaining for this quiz.");

            var attempt = new QuizAttempt
            {
                QuizId = quizId,
                LearnerUserId = learnerUserId,
                StartedAt = now,
                Status = QuizAttemptStatus.InProgress
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(attempt.Id);
        }

        public async Task<QuizAttemptTakeViewModel?> GetAttemptForTakingAsync(int attemptId, int learnerUserId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz).ThenInclude(q => q.Questions.OrderBy(qq => qq.DisplayOrder))
                    .ThenInclude(qq => qq.Options.OrderBy(o => o.DisplayOrder))
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.LearnerUserId == learnerUserId);

            if (attempt == null || attempt.Status != QuizAttemptStatus.InProgress)
                return null;

            return new QuizAttemptTakeViewModel
            {
                AttemptId = attempt.Id,
                QuizTitle = attempt.Quiz.Title,
                StartedAt = attempt.StartedAt,
                DurationMinutes = attempt.Quiz.DurationMinutes,
                Questions = attempt.Quiz.Questions.Select(q => new QuizAttemptQuestionViewModel
                {
                    QuestionId = q.Id,
                    QuestionType = q.QuestionType,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    HasImage = q.ImagePath != null,
                    Options = q.Options.Select(o => new QuizAttemptOptionViewModel
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<StudyMaterialResult> SubmitAttemptAsync(QuizAttemptSubmitViewModel model, int learnerUserId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz).ThenInclude(q => q.Questions).ThenInclude(qq => qq.Options)
                .FirstOrDefaultAsync(a => a.Id == model.AttemptId && a.LearnerUserId == learnerUserId);

            if (attempt == null)
                return StudyMaterialResult.Failure("Attempt not found.");

            if (attempt.Status != QuizAttemptStatus.InProgress)
                return StudyMaterialResult.Failure("This attempt has already been submitted.");

            var questionsById = attempt.Quiz.Questions.ToDictionary(q => q.Id);

            foreach (var answerModel in model.Answers)
            {
                if (!questionsById.TryGetValue(answerModel.QuestionId, out var question))
                    continue; // ignore answers for questions not on this quiz — defensive against tampering

                var answer = new QuizAttemptAnswer
                {
                    QuizAttemptId = attempt.Id,
                    QuizQuestionId = question.Id,
                    TextAnswer = answerModel.TextAnswer,
                    BoolAnswer = answerModel.BoolAnswer,
                    QuestionTextSnapshot = question.QuestionText
                };

                switch (question.QuestionType)
                {
                    case QuizQuestionType.MultipleChoiceSingle:
                    case QuizQuestionType.MultipleChoiceMultiple:
                        var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                        var selectedIds = answerModel.SelectedOptionIds.ToHashSet();

                        foreach (var optId in selectedIds)
                        {
                            if (question.Options.Any(o => o.Id == optId))
                            {
                                answer.SelectedOptions.Add(new QuizAttemptSelectedOption { QuizQuestionOptionId = optId });
                            }
                        }

                        // All-or-nothing marking (Decision Log) — exact set match required.
                        answer.MarksAwarded = correctOptionIds.SetEquals(selectedIds) ? question.Marks : 0;
                        break;

                    case QuizQuestionType.TrueFalse:
                        answer.MarksAwarded = (answerModel.BoolAnswer == question.CorrectBoolAnswer) ? question.Marks : 0;
                        break;

                    case QuizQuestionType.ShortAnswer:
                    case QuizQuestionType.Essay:
                    case QuizQuestionType.NumericEquation:
                        // Marked below, after the answer is persisted — nothing-lost principle:
                        // the raw TextAnswer is saved regardless of whether AI marking succeeds.
                        break;
                }

                attempt.Answers.Add(answer);
            }

            attempt.Status = QuizAttemptStatus.Submitted;
            attempt.SubmittedAt = DateTime.UtcNow;
            attempt.IsAutoSubmitted = model.IsAutoSubmit;
            attempt.TotalPossibleMarks = attempt.Quiz.Questions.Sum(q => q.Marks);

            // Save the raw answers first — this is the "nothing ever lost" point. AI marking
            // happens after, and if it fails for one answer, the answer itself is already safe.
            await _context.SaveChangesAsync();

            foreach (var answer in attempt.Answers)
            {
                var question = questionsById[answer.QuizQuestionId];
                if (question.QuestionType is QuizQuestionType.ShortAnswer or QuizQuestionType.Essay or QuizQuestionType.NumericEquation)
                {
                    var (marks, feedback) = await _markingService.MarkFreeTextAnswerAsync(question, answer.TextAnswer ?? "");
                    answer.MarksAwarded = marks;
                    answer.AiFeedback = feedback;
                }
            }

            attempt.TotalMarksAwarded = attempt.Answers.Sum(a => a.MarksAwarded ?? 0);
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(attempt.Id);
        }

        public async Task<QuizAttemptResultViewModel?> GetResultAsync(int attemptId, int learnerUserId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Include(a => a.Answers).ThenInclude(ans => ans.QuizQuestion)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.LearnerUserId == learnerUserId);

            if (attempt == null || attempt.Status != QuizAttemptStatus.Submitted)
                return null;

            return new QuizAttemptResultViewModel
            {
                AttemptId = attempt.Id,
                QuizTitle = attempt.Quiz.Title,
                TotalMarksAwarded = attempt.TotalMarksAwarded,
                TotalPossibleMarks = attempt.TotalPossibleMarks,
                IsAutoSubmitted = attempt.IsAutoSubmitted,
                Answers = attempt.Answers.Select(a => new QuizAttemptAnswerResultViewModel
                {
                    QuestionText = a.QuestionTextSnapshot ?? a.QuizQuestion.QuestionText,
                    QuestionType = a.QuizQuestion.QuestionType,
                    Marks = a.QuizQuestion.Marks,
                    MarksAwarded = a.MarksAwarded,
                    AiFeedback = a.AiFeedback
                }).ToList()
            };
        }

        private async Task<bool> IsLearnerAllowedAsync(Quiz quiz, int learnerUserId)
        {
            switch (quiz.AudienceType)
            {
                case QuizzTypes.AllEnrolledInSubject:
                    return await _context.LearnerSubjects
                        .AnyAsync(ls => ls.LearnerUserId == learnerUserId && ls.SubjectId == quiz.StudyMaterial.SubjectId);

                case QuizzTypes.SpecificLearners:
                    return quiz.AudienceLearners.Any(al => al.LearnerUserId == learnerUserId);

                case QuizzTypes.SpecificOffering:
                    // Known gap (documented earlier): Enrollment isn't built, so an Offering's
                    // actual learner list can't be resolved. Blocked rather than guessed.
                    return false;

                default:
                    return false;
            }
        }

        public async Task<List<QuizResultSummaryViewModel>> GetResultsHistoryAsync(int learnerUserId)
        {
            return await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Where(a => a.LearnerUserId == learnerUserId && a.Status == QuizAttemptStatus.Submitted)
                .OrderByDescending(a => a.SubmittedAt)
                .Select(a => new QuizResultSummaryViewModel
                {
                    AttemptId = a.Id,
                    QuizTitle = a.Quiz.Title,
                    SubmittedAt = a.SubmittedAt!.Value,
                    TotalMarksAwarded = a.TotalMarksAwarded,
                    TotalPossibleMarks = a.TotalPossibleMarks,
                    IsAutoSubmitted = a.IsAutoSubmitted
                })
                .ToListAsync();
        }
    }
}