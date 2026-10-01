using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Services;
using Tutor_Manager.Services.Notifications;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TutorManagementController : Controller
    {
        private readonly ITutorManagementService _tutorService;
        private readonly INotificationService _notificationService;
        private readonly IAiRecommendationService _aiService;

        public TutorManagementController(ITutorManagementService tutorService, INotificationService notificationService, IAiRecommendationService aiService)
        {
            _tutorService = tutorService;
            _notificationService = notificationService;
            _aiService = aiService;
        }

        // GET: /TutorManagement
        public async Task<IActionResult> Index()
        {
            var tutors = await _tutorService.GetAllTutorsAsync();
            return View(tutors);
        }

        // GET: /TutorManagement/Subjects/5
        public async Task<IActionResult> Subjects(int id)
        {
            var model = await _tutorService.GetSubjectAssignmentsAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /TutorManagement/ToggleSubject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSubject(TutorSubjectToggleViewModel model)
        {
            var (success, error) = await _tutorService.ToggleSubjectAssignmentAsync(model);

            if (!success)
                TempData["Error"] = error;

            return RedirectToAction(nameof(Subjects), new { id = model.TutorUserId });
        }

        // GET: /TutorManagement/Availability/5
        public async Task<IActionResult> Availability(int id)
        {
            var model = await _tutorService.GetAvailabilityAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /TutorManagement/AddAvailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(AddAvailabilitySlotViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid availability slot.";
                return RedirectToAction(nameof(Availability), new { id = model.TutorUserId });
            }

            try
            {
                await _tutorService.AddAvailabilitySlotAsync(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Availability), new { id = model.TutorUserId });
        }

        // POST: /TutorManagement/RemoveAvailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAvailability(int tutorAvailabilityId, int tutorUserId)
        {
            await _tutorService.RemoveAvailabilitySlotAsync(tutorAvailabilityId);
            return RedirectToAction(nameof(Availability), new { id = tutorUserId });
        }

        // GET: /TutorManagement/Unavailability/5
        public async Task<IActionResult> Unavailability(int id)
        {
            var model = await _tutorService.GetUnavailabilityAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /TutorManagement/AddUnavailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnavailability(AddUnavailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid leave entry.";
                return RedirectToAction(nameof(Unavailability), new { id = model.TutorUserId });
            }

            try
            {
                await _tutorService.AddUnavailabilityAsync(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Unavailability), new { id = model.TutorUserId });
        }

        // POST: /TutorManagement/RemoveUnavailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUnavailability(int tutorUnavailabilityId, int tutorUserId)
        {
            await _tutorService.RemoveUnavailabilityAsync(tutorUnavailabilityId);
            return RedirectToAction(nameof(Unavailability), new { id = tutorUserId });
        }

        // POST: /TutorManagement/ToggleActive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            var tutorName = await _tutorService.SetTutorActiveStatusAsync(id, isActive);

            await _notificationService.SendAsync(
                id,
                isActive ? NotificationType.TutorReactivated : NotificationType.TutorDeactivated,
                new Dictionary<string, string>());

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateForAllUnassigned()
        {
            var tutors = await _tutorService.GetAllTutorsAsync();
            var ids = tutors.Select(t => t.TutorUserId).ToList();

            var results = await _aiService.GenerateForTutorsAsync(ids);

            TempData["Success"] = $"Generated recommendations for {results.Count} of {ids.Count} tutor(s).";
            return RedirectToAction(nameof(AiRecommendations));
        }
        // POST: /TutorManagement/GenerateForTutor
        // Individual entry point — generates a recommendation for one tutor from their
        // management (Subjects) page.
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

            return RedirectToAction(nameof(Subjects), new { id = tutorUserId });
        }

        // GET: /TutorManagement/AiRecommendations
        public async Task<IActionResult> AiRecommendations()
        {
            var tutors = await _tutorService.GetAllTutorsAsync();
            var cards = new List<TutorRecommendationCardViewModel>();

            foreach (var tutor in tutors)
            {
                var recommendation = await _aiService.GetLatestRecommendationAsync(tutor.TutorUserId);
                cards.Add(new TutorRecommendationCardViewModel
                {
                    TutorUserId = tutor.TutorUserId,
                    TutorName = tutor.FullName,
                    Recommendation = recommendation
                });
            }

            return View(cards);
        }
    }
}