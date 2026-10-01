using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Services;
using Tutor_Manager.Services.Notifications;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AcademicController : Controller
    {
        private readonly IAcademicService _academicService;
        private readonly INotificationService _notificationService;

        public AcademicController(IAcademicService academicService, INotificationService notificationService)
        {
            _academicService = academicService;
            _notificationService = notificationService;
        }

        // GET: /Academic
        public async Task<IActionResult> Index()
        {
            var subjects = await _academicService.GetAllSubjectsAsync();
            return View(subjects);
        }

        // GET: /Academic/Create
        public IActionResult Create()
        {
            return View(new SubjectEditViewModel { IsActive = true });
        }

        // POST: /Academic/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _academicService.CreateSubjectAsync(model);
            TempData["Success"] = $"{model.SubjectName} created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Academic/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _academicService.GetSubjectForEditAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /Academic/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectEditViewModel model)
        {
            if (id != model.SubjectId) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            await _academicService.UpdateSubjectAsync(model);
            TempData["Success"] = $"{model.SubjectName} updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Academic/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            var subjectName = await _academicService.SetSubjectActiveStatusAsync(id, isActive);

            if (!isActive)
            {
                var affectedTutorUserIds = await _academicService.GetTutorUserIdsForSubjectAsync(id);

                foreach (var tutorUserId in affectedTutorUserIds)
                {
                    await _notificationService.SendAsync(tutorUserId, NotificationType.SubjectDeactivated,
                        new Dictionary<string, string> { { "SubjectName", subjectName } });
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}