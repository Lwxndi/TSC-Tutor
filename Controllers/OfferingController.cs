using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services;
using Tutor_Manager.Services.Academic;
using Tutor_Manager.Services.Email;
using Tutor_Manager.Services.Notifications;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OfferingController : Controller
    {
        private readonly IOfferingService _offeringService;
        private readonly ISessionService _sessionService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public OfferingController(
            IOfferingService offeringService,
            INotificationService notificationService,
            ISessionService sessionService,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _offeringService = offeringService;
            _sessionService = sessionService;
            _notificationService = notificationService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<IActionResult> Index(OfferingFilterViewModel filter)
        {
            var offerings = await _offeringService.GetAllOfferingsAsync(filter);
            ViewBag.Subjects = await _offeringService.GetSubjectDropdownAsync(); // note: this is currently private — see below
            ViewBag.Tutors = await _offeringService.GetAllTutorsForFilterAsync();
            ViewBag.Filter = filter;
            return View(offerings);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _offeringService.GetCreateFormDataAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OfferingEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var formData = await _offeringService.GetCreateFormDataAsync();
                model.AvailableSubjects = formData.AvailableSubjects;
                model.AvailableTutors = formData.AvailableTutors;
                return View(model);
            }

            var (success, error, offeringId) = await _offeringService.CreateOfferingAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                var formData = await _offeringService.GetCreateFormDataAsync();
                model.AvailableSubjects = formData.AvailableSubjects;
                model.AvailableTutors = formData.AvailableTutors;
                return View(model);
            }

            await NotifyOfferingAssignedAsync(offeringId!.Value, model.TutorUserId);

            TempData["Success"] = "Offering created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _offeringService.GetForEditAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OfferingEditViewModel model)
        {
            if (id != model.OfferingId) return BadRequest();

            if (!ModelState.IsValid)
            {
                var formData = await _offeringService.GetCreateFormDataAsync();
                model.AvailableSubjects = formData.AvailableSubjects;
                model.AvailableTutors = formData.AvailableTutors;
                return View(model);
            }

            var existing = await _offeringService.GetForEditAsync(id);
            var oldTutorUserId = existing?.TutorUserId;

            var (success, error) = await _offeringService.UpdateOfferingAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                var formData = await _offeringService.GetCreateFormDataAsync();
                model.AvailableSubjects = formData.AvailableSubjects;
                model.AvailableTutors = formData.AvailableTutors;
                return View(model);
            }

            // NEW: keep generated Sessions in sync with whatever changed on this edit.
            var (removed, created, skipped) = await _sessionService.RegenerateFutureSessionsForOfferingAsync(id);

            if (oldTutorUserId.HasValue && oldTutorUserId.Value != model.TutorUserId)
            {
                await NotifyOfferingDeactivatedAsync(id, oldTutorUserId.Value);
                await NotifyOfferingAssignedAsync(id, model.TutorUserId);
            }

            TempData["Success"] = skipped > 0
                ? $"Offering updated. {removed} future session(s) replaced, {created} regenerated, {skipped} skipped due to conflicts."
                : $"Offering updated and {created} session(s) regenerated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            var subjectName = await _offeringService.SetOfferingActiveStatusAsync(id, isActive);

            if (!isActive)
            {
                var offering = await _offeringService.GetForEditAsync(id);
                if (offering != null)
                {
                    await NotifyOfferingDeactivatedAsync(id, offering.TutorUserId);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task NotifyOfferingAssignedAsync(int offeringId, int tutorUserId)
        {
            var offering = await _offeringService.GetForEditAsync(offeringId);
            if (offering == null) return;

            var tutor = offering.AvailableTutors.FirstOrDefault(t => t.TutorUserId == tutorUserId);
            var subjectName = offering.AvailableSubjects.FirstOrDefault(s => s.SubjectId == offering.SubjectId)?.SubjectName ?? "Subject";
            var firstName = tutor?.FullName.Split(' ').FirstOrDefault() ?? "there";

            var data = new Dictionary<string, string>
    {
        { "Subject", subjectName },
        { "Grade", ((int)offering.Grade).ToString() },
        { "Type", offering.Type.ToString() },
        { "TeachingDays", string.Join(", ", offering.SelectedTeachingDays) },
        { "DeliveryMethod", offering.DeliveryMethod.ToString() },
        { "FirstName", firstName }
    };

            await _notificationService.SendAsync(tutorUserId, NotificationType.OfferingAssigned, data);

            var tutorEmail = await _offeringService.GetTutorEmailAsync(tutorUserId);
            if (!string.IsNullOrEmpty(tutorEmail))
            {
                var message = _emailTemplateService.Build(EmailType.OfferingAssigned, tutorEmail, data);
                await _emailService.SendAsync(message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetValidStartTimes(DeliveryMethod deliveryMethod, OfferingType type, List<DayOfWeek> days, int durationMinutes)
        {
            var times = await _offeringService.GetValidStartTimesAsync(deliveryMethod, type, days, durationMinutes);
            return Json(times);
        }

       



        private async Task NotifyOfferingDeactivatedAsync(int offeringId, int tutorUserId)
        {
            var offering = await _offeringService.GetForEditAsync(offeringId);
            if (offering == null) return;

            var tutor = offering.AvailableTutors.FirstOrDefault(t => t.TutorUserId == tutorUserId);
            var subjectName = offering.AvailableSubjects.FirstOrDefault(s => s.SubjectId == offering.SubjectId)?.SubjectName ?? "Subject";
            var firstName = tutor?.FullName.Split(' ').FirstOrDefault() ?? "there";

            var data = new Dictionary<string, string>
            {
                { "Subject", subjectName },
                { "Grade", ((int)offering.Grade).ToString() },
                { "FirstName", firstName }
            };

            await _notificationService.SendAsync(tutorUserId, NotificationType.OfferingDeactivated, data);

            var tutorEmail = await _offeringService.GetTutorEmailAsync(tutorUserId);
            if (!string.IsNullOrEmpty(tutorEmail))
            {
                var message = _emailTemplateService.Build(EmailType.OfferingDeactivated, tutorEmail, data);
                await _emailService.SendAsync(message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetOfferingsForSubjectGrade(int subjectId, Grade grade)
        {
            var offerings = await _offeringService.GetOfferingsForSubjectGradeAsync(subjectId, grade);

            var result = offerings.Select(o => new
            {
                tutorName = o.TutorName,
                type = o.Type.ToString(),
                deliveryMethod = o.DeliveryMethod.ToString(),
                teachingDays = o.TeachingDays.Select(d => d.ToString()),
                isActive = o.IsActive
            });

            return Json(result);
        }

        // GET: /Offering/GetEligibleTutors?subjectId=1&grade=11
        [HttpGet]
        public async Task<IActionResult> GetEligibleTutors(int subjectId, Grade? grade)
        {
            var tutors = await _offeringService.GetEligibleTutorsAsync(subjectId, grade);
            return Json(tutors);
        }

        [HttpGet]
        public async Task<IActionResult> GetOfferingsForSubject(int subjectId)
        {
            var offerings = await _offeringService.GetOfferingsForSubjectAsync(subjectId);

            var result = offerings.Select(o => new
            {
                grade = ((int)o.Grade),
                tutorName = o.TutorName,
                type = o.Type.ToString(),
                deliveryMethod = o.DeliveryMethod.ToString(),
                teachingDays = o.TeachingDays.Select(d => d.ToString()),
                isActive = o.IsActive
            });

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> CheckTutorLoad([FromBody] OfferingEditViewModel model)
        {
            if (model == null)
                return BadRequest(new { isValid = false, error = "Invalid request payload." });

            var (isValid, error) = await _offeringService.ValidateTutorLoadAsync(
                model.TutorUserId, model.SubjectId, model.Type, model.DeliveryMethod,
                model.SelectedTeachingDays, model.DayOverrides, model.WeekdayStartTime,
                model.WeekendStartTime, model.StartTime, model.DurationMinutes,
                model.OfferingId == 0 ? null : model.OfferingId);

            return Json(new { isValid, error });
        }

    }
}