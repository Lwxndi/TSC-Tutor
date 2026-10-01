// Controllers/QuizController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Helpers;
using Tutor_Manager.Models;
using Tutor_Manager.Services.QuizzServices;
using Tutor_Manager.ViewModels.QuizzViewmodels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Tutor")]
    public class QuizController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IQuizService _quizService;
        private readonly IQuizGenerationService _quizGenerationService;
        private readonly IQuizQuestionService _quizQuestionService;

        public QuizController(Tutor_ManagerDatabaseContext context, IQuizService quizService, IQuizGenerationService quizGenerationService, IQuizQuestionService quizQuestionService)
        {
            _context = context;
            _quizService = quizService;
            _quizGenerationService = quizGenerationService;
            _quizQuestionService = quizQuestionService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = new QuizCreateViewModel();
            await PopulateDropdownsAsync(model, tutorUserId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizCreateViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model, tutorUserId);
                return View(model);
            }

            var result = await _quizService.CreateAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await PopulateDropdownsAsync(model, tutorUserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Quiz created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var quizzes = await _context.Quizzes
                .Where(q => q.TutorUserId == tutorUserId && q.IsActive)
                .Include(q => q.StudyMaterial)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(quizzes);
        }

        private async Task PopulateDropdownsAsync(QuizCreateViewModel model, int tutorUserId)
        {
            var materials = await _context.StudyMaterials
                .Where(m => m.TutorUserId == tutorUserId && m.IsActive)
                .Select(m => new { m.Id, m.Title, m.SubjectId })
                .ToListAsync();
            ViewBag.MaterialsWithSubject = materials;

            var offerings = await _context.Offerings
                .Where(o => o.TutorUserId == tutorUserId && o.IsActive)
                .Select(o => new { o.OfferingId, o.SubjectId, o.Subject.SubjectName, o.Grade, o.Type })
                .ToListAsync();
            ViewBag.OfferingsWithSubject = offerings;

            var tutorSubjectIds = await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .Select(ts => ts.SubjectId)
                .Distinct()
                .ToListAsync();

            var learners = await _context.LearnerSubjects
                .Where(ls => tutorSubjectIds.Contains(ls.SubjectId))
                .Select(ls => new
                {
                    ls.LearnerUserId,
                    ls.SubjectId,
                    FullName = ls.Learner.User.FirstName + " " + ls.Learner.User.LastName
                })
                .Distinct()
                .ToListAsync();
            ViewBag.LearnersWithSubject = learners;
        }

        private async Task PopulateDropdownsAsync(QuizEditViewModel model, int tutorUserId)
        {
            var materials = await _context.StudyMaterials
                .Where(m => m.TutorUserId == tutorUserId && m.IsActive)
                .Select(m => new { m.Id, m.Title, m.SubjectId })
                .ToListAsync();
            ViewBag.MaterialsWithSubject = materials;

            var offerings = await _context.Offerings
                .Where(o => o.TutorUserId == tutorUserId && o.IsActive)
                .Select(o => new { o.OfferingId, o.SubjectId, o.Subject.SubjectName, o.Grade, o.Type })
                .ToListAsync();
            ViewBag.OfferingsWithSubject = offerings;

            var tutorSubjectIds = await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .Select(ts => ts.SubjectId)
                .Distinct()
                .ToListAsync();

            var learners = await _context.LearnerSubjects
                .Where(ls => tutorSubjectIds.Contains(ls.SubjectId))
                .Select(ls => new
                {
                    ls.LearnerUserId,
                    ls.SubjectId,
                    FullName = ls.Learner.User.FirstName + " " + ls.Learner.User.LastName
                })
                .Distinct()
                .ToListAsync();
            ViewBag.LearnersWithSubject = learners;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var quiz = await _quizService.GetByIdAsync(id, tutorUserId);

            if (quiz == null)
                return NotFound();

            if (quiz.OpenAt <= DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "This quiz has already opened and can no longer be edited.";
                return RedirectToAction(nameof(Index));
            }

            // quiz.OpenAt/CloseAt are stored as true UTC — convert back to SAST wall-clock
            // before splitting into Date/Time fields, or the edit form will show the wrong time.
            var openSast = SouthAfricaTime.ToSast(quiz.OpenAt);
            var closeSast = SouthAfricaTime.ToSast(quiz.CloseAt);

            var model = new QuizEditViewModel
            {
                Id = quiz.Id,
                StudyMaterialId = quiz.StudyMaterialId,
                Title = quiz.Title,
                OpenDate = DateOnly.FromDateTime(openSast),
                OpenTime = TimeOnly.FromDateTime(openSast),
                CloseDate = DateOnly.FromDateTime(closeSast),
                CloseTime = TimeOnly.FromDateTime(closeSast),
                DurationMinutes = quiz.DurationMinutes,
                MaxAttempts = quiz.MaxAttempts,
                AudienceType = quiz.AudienceType,
                OfferingId = quiz.OfferingId,
                ExistingLearnerUserIds = quiz.AudienceLearners.Select(al => al.LearnerUserId).ToList(),
                SelectedLearnerUserIds = quiz.AudienceLearners.Select(al => al.LearnerUserId).ToList()
            };

            await PopulateDropdownsAsync(model, tutorUserId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuizEditViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model, tutorUserId);
                return View(model);
            }

            var result = await _quizService.UpdateAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await PopulateDropdownsAsync(model, tutorUserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Quiz updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _quizService.DeleteAsync(id, tutorUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? "Quiz deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _quizService.BulkDeleteAsync(ids ?? new List<int>(), tutorUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? $"{result.StudyMaterialId} item(s) deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _quizGenerationService.GenerateQuestionsAsync(id, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Questions generated successfully."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Review(int quizId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var quiz = await _quizQuestionService.GetQuizForReviewAsync(quizId, tutorUserId);

            if (quiz == null)
                return NotFound();

            var model = new QuizReviewViewModel
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                Status = quiz.Status,
                Questions = quiz.Questions.Select(q => new QuizQuestionSummaryViewModel
                {
                    Id = q.Id,
                    QuestionType = q.QuestionType,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    DisplayOrder = q.DisplayOrder,
                    HasImage = q.ImagePath != null,
                    CorrectBoolAnswer = q.CorrectBoolAnswer,
                    MarkingGuidance = q.MarkingGuidance,
                    Options = q.Options
                        .OrderBy(o => o.DisplayOrder)
                        .Select(o => new QuizOptionSummaryViewModel
                        {
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect,
                            DisplayOrder = o.DisplayOrder
                        }).ToList()
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(int quizId, int questionId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var question = await _quizQuestionService.GetQuestionAsync(questionId, quizId, tutorUserId);

            if (question == null)
                return NotFound();

            var model = new QuizQuestionEditViewModel
            {
                Id = question.Id,
                QuizId = quizId,
                QuestionType = question.QuestionType,
                QuestionText = question.QuestionText,
                Marks = question.Marks,
                MarkingGuidance = question.MarkingGuidance,
                CorrectBoolAnswer = question.CorrectBoolAnswer,
                HasImage = question.ImagePath != null,
                Options = question.Options.Select(o => new QuizQuestionOptionEditViewModel
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(QuizQuestionEditViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
                return View(model);

            var result = await _quizQuestionService.UpdateQuestionAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Question updated.";
            return RedirectToAction(nameof(Review), new { quizId = model.QuizId });
        }

        [HttpGet]
        public IActionResult AddQuestion(int quizId)
        {
            var model = new QuizQuestionEditViewModel
            {
                QuizId = quizId,
                QuestionType = QuizQuestionType.MultipleChoiceSingle,
                Options = new List<QuizQuestionOptionEditViewModel> { new(), new() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(QuizQuestionEditViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
                return View(model);

            var result = await _quizQuestionService.AddQuestionAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Question added.";
            return RedirectToAction(nameof(Review), new { quizId = model.QuizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int quizId, int questionId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _quizQuestionService.DeleteQuestionAsync(questionId, quizId, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Question deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Review), new { quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int quizId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _quizQuestionService.ConfirmAsync(quizId, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Quiz confirmed and ready."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Review), new { quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unconfirm(int quizId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _quizQuestionService.UnconfirmAsync(quizId, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Quiz un-confirmed — you can edit questions again."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Review), new { quizId });
        }

        private async Task PopulateDropdownsAsync(int tutorUserId)
        {
            var offerings = await _context.Offerings
                .Where(o => o.TutorUserId == tutorUserId && o.IsActive)
                .Select(o => new { o.OfferingId, o.SubjectId, o.Subject.SubjectName, o.Grade, o.Type })
                .ToListAsync();
            ViewBag.OfferingsWithSubject = offerings;

            var tutorSubjectIds = await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .Select(ts => ts.SubjectId)
                .Distinct()
                .ToListAsync();

            var learners = await _context.LearnerSubjects
                .Where(ls => tutorSubjectIds.Contains(ls.SubjectId))
                .Select(ls => new
                {
                    ls.LearnerUserId,
                    ls.SubjectId,
                    FullName = ls.Learner.User.FirstName + " " + ls.Learner.User.LastName
                })
                .Distinct()
                .ToListAsync();
            ViewBag.LearnersWithSubject = learners;
        }
    }
}