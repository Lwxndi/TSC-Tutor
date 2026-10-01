using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Models;
using Tutor_Manager.Services;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TutorAssignmentController : Controller
    {
        private readonly ITutorAssignmentService _assignmentService;
        private readonly IAiRecommendationService _aiService;

        public TutorAssignmentController(ITutorAssignmentService assignmentService, IAiRecommendationService aiService)
        {
            _assignmentService = assignmentService;
            _aiService = aiService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> Index()
        {
            var model = await _assignmentService.GetGroupedTutorsAsync();
            return View(model);
        }

        public async Task<IActionResult> ManageTutor(int id)
        {
            var model = await _assignmentService.GetManageTutorAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateForTutor(int tutorUserId)
        {
            try
            {
                await _aiService.GenerateRecommendationAsync(tutorUserId);
                TempData["Success"] = "Recommendation generated.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"AI recommendation failed: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageTutor), new { id = tutorUserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateForAllUnassigned()
        {
            var grouped = await _assignmentService.GetGroupedTutorsAsync();
            var ids = grouped.Unassigned.Select(t => t.TutorUserId).ToList();

            var results = await _aiService.GenerateForTutorsAsync(ids);

            TempData["Success"] = results.Count == ids.Count
                ? $"Generated recommendations for {results.Count} tutor(s)."
                : $"Generated {results.Count} of {ids.Count} recommendation(s) — {ids.Count - results.Count} failed. Check logs.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int tutorUserId, int subjectId, Grade grade)
        {
            var (success, error) = await _assignmentService.AssignAsync(tutorUserId, subjectId, grade, CurrentUserId);

            if (!success)
                TempData["Error"] = error;

            return RedirectToAction(nameof(ManageTutor), new { id = tutorUserId });
        }
    }
}