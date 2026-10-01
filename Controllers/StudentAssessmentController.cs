using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tutor_Manager.Services.AssessmentServices;
using Tutor_Manager.ViewModels.AssessmentViewmodels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Learner")]
    public class StudentAssessmentController : Controller
    {
        private readonly IAssessmentAttemptService _attemptService;

        public StudentAssessmentController(IAssessmentAttemptService attemptService)
        {
            _attemptService = attemptService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var assessments = await _attemptService.GetAvailableAssessmentsAsync(learnerUserId);
            return View(assessments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int assessmentId)
        {
            var learnerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _attemptService.StartOrResumeAttemptAsync(assessmentId, learnerUserId);

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
        public async Task<IActionResult> Submit(AssessmentAttemptSubmitViewModel model)
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
    }
}