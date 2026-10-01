using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.AssessmentViewmodels;

namespace Tutor_Manager.Services.AssessmentServices
{
    public class AssessmentAttemptService : IAssessmentAttemptService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IAssessmentAttemptMarkingService _markingService;

        public AssessmentAttemptService(Tutor_ManagerDatabaseContext context, IAssessmentAttemptMarkingService markingService)
        {
            _context = context;
            _markingService = markingService;
        }

        public async Task<List<AvailableAssessmentViewModel>> GetAvailableAssessmentsAsync(int learnerUserId)
        {
            var now = DateTime.UtcNow;

            var candidates = await _context.Assessments
                .Include(a => a.AudienceLearners)
                .Where(a => a.IsActive
                    && a.Status == AssessmentGenerationStatus.Confirmed
                    && a.OpenAt <= now
                    && a.CloseAt >= now)
                .ToListAsync();

            var result = new List<AvailableAssessmentViewModel>();

            foreach (var assessment in candidates)
            {
                if (!await IsLearnerAllowedAsync(assessment, learnerUserId))
                    continue;

                var submittedCount = await _context.AssessmentAttempts
                    .CountAsync(a => a.AssessmentId == assessment.Id && a.LearnerUserId == learnerUserId && a.Status == AssessmentAttemptStatus.Submitted);

                var inProgress = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.AssessmentId == assessment.Id && a.LearnerUserId == learnerUserId && a.Status == AssessmentAttemptStatus.InProgress);

                result.Add(new AvailableAssessmentViewModel
                {
                    AssessmentId = assessment.Id,
                    Title = assessment.Title,
                    OpenAt = assessment.OpenAt,
                    CloseAt = assessment.CloseAt,
                    MaxAttempts = assessment.MaxAttempts,
                    SubmittedAttemptCount = submittedCount,
                    HasInProgressAttempt = inProgress != null,
                    InProgressAttemptId = inProgress?.Id
                });
            }

            return result;
        }

        public async Task<StudyMaterialResult> StartOrResumeAttemptAsync(int assessmentId, int learnerUserId)
        {
            var assessment = await _context.Assessments
                .Include(a => a.AudienceLearners)
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.IsActive);

            if (assessment == null)
                return StudyMaterialResult.Failure("Assessment not found.");

            if (assessment.Status != AssessmentGenerationStatus.Confirmed)
                return StudyMaterialResult.Failure("This assessment is not currently available.");

            var now = DateTime.UtcNow;
            if (now < assessment.OpenAt || now > assessment.CloseAt)
                return StudyMaterialResult.Failure("This assessment is not currently open.");

            if (!await IsLearnerAllowedAsync(assessment, learnerUserId))
                return StudyMaterialResult.Failure("You do not have access to this assessment.");

            var existing = await _context.AssessmentAttempts
                .FirstOrDefaultAsync(a => a.AssessmentId == assessmentId && a.LearnerUserId == learnerUserId && a.Status == AssessmentAttemptStatus.InProgress);

            if (existing != null)
                return StudyMaterialResult.Success(existing.Id); // resume — no per-attempt duration/deadline exists for Assessment

            var submittedCount = await _context.AssessmentAttempts
                .CountAsync(a => a.AssessmentId == assessmentId && a.LearnerUserId == learnerUserId && a.Status == AssessmentAttemptStatus.Submitted);

            if (submittedCount >= assessment.MaxAttempts)
                return StudyMaterialResult.Failure("You have no attempts remaining for this assessment.");

            var attempt = new AssessmentAttempt
            {
                AssessmentId = assessmentId,
                LearnerUserId = learnerUserId,
                StartedAt = now,
                Status = AssessmentAttemptStatus.InProgress
            };

            _context.AssessmentAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(attempt.Id);
        }

        public async Task<AssessmentAttemptTakeViewModel?> GetAttemptForTakingAsync(int attemptId, int learnerUserId)
        {
            var attempt = await _context.AssessmentAttempts
                .Include(a => a.Assessment).ThenInclude(asmt => asmt.Questions.OrderBy(q => q.DisplayOrder))
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.LearnerUserId == learnerUserId);

            if (attempt == null || attempt.Status != AssessmentAttemptStatus.InProgress)
                return null;

            return new AssessmentAttemptTakeViewModel
            {
                AttemptId = attempt.Id,
                AssessmentTitle = attempt.Assessment.Title,
                Questions = attempt.Assessment.Questions.Select(q => new AssessmentAttemptQuestionViewModel
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    DisplayOrder = q.DisplayOrder
                }).ToList()
            };
        }

        public async Task<StudyMaterialResult> SubmitAttemptAsync(AssessmentAttemptSubmitViewModel model, int learnerUserId)
        {
            var attempt = await _context.AssessmentAttempts
                .Include(a => a.Assessment).ThenInclude(asmt => asmt.Questions)
                .FirstOrDefaultAsync(a => a.Id == model.AttemptId && a.LearnerUserId == learnerUserId);

            if (attempt == null)
                return StudyMaterialResult.Failure("Attempt not found.");

            if (attempt.Status != AssessmentAttemptStatus.InProgress)
                return StudyMaterialResult.Failure("This attempt has already been submitted.");

            var questionsById = attempt.Assessment.Questions.ToDictionary(q => q.Id);

            foreach (var answerModel in model.Answers)
            {
                if (!questionsById.ContainsKey(answerModel.QuestionId))
                    continue; // defensive against tampering, same as Quiz

                attempt.Answers.Add(new AssessmentAttemptAnswer
                {
                    AssessmentAttemptId = attempt.Id,
                    AssessmentQuestionId = answerModel.QuestionId,
                    TextAnswer = answerModel.TextAnswer
                });
            }

            attempt.Status = AssessmentAttemptStatus.Submitted;
            attempt.SubmittedAt = DateTime.UtcNow;
            attempt.IsAutoSubmitted = model.IsAutoSubmit;
            attempt.TotalPossibleMarks = attempt.Assessment.Questions.Sum(q => q.Marks);

            // Nothing-lost point — raw answers saved before any AI call happens.
            await _context.SaveChangesAsync();

            foreach (var answer in attempt.Answers)
            {
                var question = questionsById[answer.AssessmentQuestionId];
                var (marks, feedback) = await _markingService.MarkAnswerAsync(question, answer.TextAnswer ?? "");
                answer.MarksAwarded = marks;
                answer.AiFeedback = feedback;
            }

            attempt.TotalMarksAwarded = attempt.Answers.Sum(a => a.MarksAwarded ?? 0);
            await _context.SaveChangesAsync();

            return StudyMaterialResult.Success(attempt.Id);
        }

        public async Task<AssessmentAttemptResultViewModel?> GetResultAsync(int attemptId, int learnerUserId)
        {
            var attempt = await _context.AssessmentAttempts
                .Include(a => a.Assessment)
                .Include(a => a.Answers).ThenInclude(ans => ans.AssessmentQuestion)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.LearnerUserId == learnerUserId);

            if (attempt == null || attempt.Status != AssessmentAttemptStatus.Submitted)
                return null;

            return new AssessmentAttemptResultViewModel
            {
                AttemptId = attempt.Id,
                AssessmentTitle = attempt.Assessment.Title,
                TotalMarksAwarded = attempt.TotalMarksAwarded,
                TotalPossibleMarks = attempt.TotalPossibleMarks,
                IsAutoSubmitted = attempt.IsAutoSubmitted,
                Answers = attempt.Answers.Select(a => new AssessmentAttemptAnswerResultViewModel
                {
                    QuestionId = a.AssessmentQuestionId,
                    QuestionText = a.AssessmentQuestion.QuestionText,
                    Marks = a.AssessmentQuestion.Marks,
                    MarksAwarded = a.MarksAwarded,
                    AiFeedback = a.AiFeedback
                }).ToList()
            };
        }

        private async Task<bool> IsLearnerAllowedAsync(Assessment assessment, int learnerUserId)
        {
            switch (assessment.AudienceType)
            {
                case QuizzTypes.AllEnrolledInSubject:
                    return await _context.LearnerSubjects
                        .AnyAsync(ls => ls.LearnerUserId == learnerUserId && ls.SubjectId == assessment.SubjectId);

                case QuizzTypes.SpecificLearners:
                    return assessment.AudienceLearners.Any(al => al.LearnerUserId == learnerUserId);

                case QuizzTypes.SpecificOffering:
                    // Same known gap as Quiz — Enrollment isn't built, so blocked rather than guessed.
                    return false;

                default:
                    return false;
            }
        }
    }
}