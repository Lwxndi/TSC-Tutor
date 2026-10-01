using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tutor_Manager.Services.QuizzServices;
using Tutor_Manager.ViewModels.QuizzViewmodels;


namespace Tutor_Manager.Controllers
{
    //[Authorize(Roles = "Learner")]
    public class StudentQuizController : Controller
    {
        private readonly IQuizAttemptService _attemptService;

        public StudentQuizController(IQuizAttemptService attemptService)
        {
            _attemptService = attemptService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var quizzes = await _attemptService.GetAvailableQuizzesAsync(learnerUserId);
            return View(quizzes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int quizId)
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _attemptService.StartOrResumeAttemptAsync(quizId, learnerUserId);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Take), new { attemptId = result.StudyMaterialId });
        }

        [HttpGet]
        public async Task<IActionResult> Take(int attemptId)
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = await _attemptService.GetAttemptForTakingAsync(attemptId, learnerUserId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(QuizAttemptSubmitViewModel model)
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _attemptService.SubmitAttemptAsync(model, learnerUserId);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Result), new { attemptId = result.StudyMaterialId });
        }

        [HttpGet]
        public async Task<IActionResult> Result(int attemptId)
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = await _attemptService.GetResultAsync(attemptId, learnerUserId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Results()
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var history = await _attemptService.GetResultsHistoryAsync(learnerUserId);
            return View(history);
        }
    }
}