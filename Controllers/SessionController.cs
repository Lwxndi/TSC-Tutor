using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tutor_Manager.Models;
using Tutor_Manager.Services;
using Tutor_Manager.Services.EnrollmentServices;
using Tutor_Manager.ViewModels;
using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.Controllers
{
    // NOTE: class-level [Authorize(Roles = "Admin")] removed — it was stacking
    // (AND, not OR) with the per-action role attributes on the timetable
    // actions below, making them unreachable by anyone who wasn't ALSO an
    // Admin. Every admin-only action now carries its own explicit attribute.
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IEnrollmentService _enrollmentService;

        public SessionController(ISessionService sessionService, IEnrollmentService enrollmentService)
        {
            _sessionService = sessionService;
            _enrollmentService = enrollmentService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Session  (standalone landing page)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(SessionFilterViewModel filter)
        {
            filter.DateRange = string.IsNullOrEmpty(filter.DateRange) ? "Today" : filter.DateRange;
            var model = await _sessionService.GetSessionIndexAsync(filter);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            var y = year ?? DateTime.Today.Year;
            var m = month ?? DateTime.Today.Month;

            var filter = new SessionFilterViewModel { DateRange = "All" };
            var allSessions = await _sessionService.GetSessionIndexAsync(filter);

            var monthSessions = allSessions.Sessions
                .Where(s => s.Date.Year == y && s.Date.Month == m)
                .ToList();

            ViewBag.Year = y;
            ViewBag.Month = m;
            return View(monthSessions);
        }

        // GET: /Session/ForOffering/5  (kept for the "Sessions" button on an Offering row)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ForOffering(int offeringId)
        {
            var sessions = await _sessionService.GetSessionsForOfferingAsync(offeringId);
            ViewBag.OfferingId = offeringId;
            return View("Offering", sessions);
        }

        // GET: /Session/Create  (no offeringId — show picker)
        // GET: /Session/Create?offeringId=5  (offering known — show the real form)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? offeringId)
        {
            if (!offeringId.HasValue)
            {
                var offerings = await _sessionService.GetOfferingPickerListAsync();
                return View("SelectOffering", offerings);
            }

            var model = await _sessionService.GetCreateFormDataAsync(offeringId.Value);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /Session/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSessionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateDisplayFields(model);
                return View(model);
            }

            var (success, error, sessionId) = await _sessionService.CreateSessionAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                await RepopulateDisplayFields(model);
                return View(model);
            }

            TempData["Success"] = "Session scheduled.";
            return RedirectToAction(nameof(Details), new { id = sessionId });
        }

        private async Task RepopulateDisplayFields(CreateSessionViewModel model)
        {
            var formData = await _sessionService.GetCreateFormDataAsync(model.OfferingId);
            if (formData == null) return;

            model.SubjectName = formData.SubjectName;
            model.Grade = formData.Grade;
            model.TutorName = formData.TutorName;
            model.OfferingTeachingDays = formData.OfferingTeachingDays;
            model.DeliveryMethod = formData.DeliveryMethod;
        }

        // GET: /Session/Details/5
        // Left open to any authenticated role — a tutor/learner/guardian viewing
        // their own timetable needs to click into a session's details. If this
        // needs tightening to "only involved parties," that's a follow-up.
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _sessionService.GetDetailsAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // GET: /Session/Reschedule/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reschedule(int id)
        {
            var model = await _sessionService.GetRescheduleFormDataAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: /Session/Reschedule
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(RescheduleSessionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, error) = await _sessionService.RescheduleSessionAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                return View(model);
            }

            TempData["Success"] = "Session rescheduled.";
            return RedirectToAction(nameof(Details), new { id = model.SessionId });
        }

        // GET: /Session/Cancel/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var session = await _sessionService.GetDetailsAsync(id);
            if (session == null) return NotFound();

            return View(new CancelSessionViewModel { SessionId = id });
        }

        // POST: /Session/Cancel
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelSessionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, error) = await _sessionService.CancelSessionAsync(model.SessionId, model.Reason);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                return View(model);
            }

            TempData["Success"] = "Session cancelled.";
            return RedirectToAction(nameof(Details), new { id = model.SessionId });
        }

        // GET: /Session/Generate
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Generate()
        {
            var model = new GenerateSessionsFilterViewModel
            {
                AvailableSubjects = await _sessionService.GetActiveSubjectsAsync()
            };
            return View(model);
        }

        // POST: /Session/Generate  (Subject/Grade/DateRange chosen — load matching offerings)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(GenerateSessionsFilterViewModel model)
        {
            model.AvailableSubjects = await _sessionService.GetActiveSubjectsAsync();

            if (model.SubjectId.HasValue && model.Grade.HasValue)
            {
                model.MatchingOfferings = await _sessionService.GetMatchingOfferingsAsync(model.SubjectId.Value, model.Grade.Value);
            }

            return View(model);
        }

        // POST: /Session/PreviewGeneration
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreviewGeneration(GenerateSessionsRequestViewModel request)
        {
            if (!request.SelectedOfferingIds.Any())
            {
                TempData["Error"] = "Select at least one offering.";
                return RedirectToAction(nameof(Generate));
            }

            var preview = await _sessionService.PreviewGenerationAsync(request);
            return View("Preview", preview);
        }

        // POST: /Session/ConfirmGeneration
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmGeneration(List<SessionCandidateViewModel> candidates)
        {
            var validOnly = candidates.Where(c => c.IsValid).ToList();
            var (created, skipped) = await _sessionService.ConfirmGenerationAsync(validOnly);

            TempData["Success"] = $"{created} session(s) created, {skipped} skipped.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TimetablePreview()
        {
            var model = new TimetablePreviewFilterViewModel
            {
                AvailableSubjects = await _sessionService.GetActiveSubjectsAsync(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimetablePreview(TimetablePreviewFilterViewModel model)
        {
            var preview = await _sessionService.GetSystemTimetablePreviewAsync(model.SubjectId, model.Grade, model.StartDate, model.EndDate);
            model.AvailableSubjects = await _sessionService.GetActiveSubjectsAsync();
            model.Preview = preview;
            return View(model);
        }

        // ── Substitute tutor assignment ──

        // GET: /Session/AssignSubstitute/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSubstitute(int id)
        {
            var session = await _sessionService.GetDetailsAsync(id);
            if (session == null) return NotFound();

            var model = new AssignSubstituteViewModel
            {
                SessionId = id,
                SessionSummary = session,
                AvailableSubstitutes = await _sessionService.GetAvailableSubstitutesAsync(id)
            };

            return View(model);
        }

        // POST: /Session/AssignSubstitute
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSubstitute(int sessionId, int tutorUserId)
        {
            var (success, error) = await _sessionService.AssignOverrideTutorAsync(sessionId, tutorUserId);

            if (!success)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Substitute tutor assigned for this session.";

            return RedirectToAction(nameof(Details), new { id = sessionId });
        }

        // POST: /Session/RemoveSubstitute
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSubstitute(int sessionId)
        {
            var (success, error) = await _sessionService.RemoveOverrideTutorAsync(sessionId);

            if (!success)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Substitute removed — session reverted to the regular tutor.";

            return RedirectToAction(nameof(Details), new { id = sessionId });
        }

        // ── Personal timetables (Tutor / Learner / Guardian) ──
        // These stay in SessionController per your call — no separate
        // LearnerSessionController/GuardianSessionController for now. The
        // difference in what each role sees is handled entirely by which
        // data method gets called and by the view, not by controller identity.

        // GET: /Session/MyTimetable?startDate=...&endDate=...
        [HttpGet]
        //[Authorize(Roles = "Tutor")]
        public async Task<IActionResult> MyTimetable(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? start.AddDays(7);

            var sessions = await _sessionService.GetTutorTimetableAsync(CurrentUserId, start, end);
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            return View(sessions);
        }

        // GET: /Session/LearnerTimetable/5?startDate=...&endDate=...
        [HttpGet]
        [Authorize(Roles = "Learner,Parent,Admin")]
        public async Task<IActionResult> LearnerTimetable(int? learnerUserId, DateTime? startDate, DateTime? endDate)
        {
            int targetLearnerId;

            if (User.IsInRole("Learner"))
            {
                targetLearnerId = CurrentUserId;
                if (learnerUserId.HasValue && learnerUserId.Value != CurrentUserId)
                    return Forbid();
            }
            else if (User.IsInRole("Parent"))
            {
                if (!learnerUserId.HasValue)
                    return RedirectToAction(nameof(MyChildrensTimetable));

                var linkedEnrollments = await _enrollmentService.GetForGuardianAsync(CurrentUserId);
                var isLinked = linkedEnrollments.Any(e => e.LearnerUserId == learnerUserId.Value);
                if (!isLinked)
                    return Forbid();

                targetLearnerId = learnerUserId.Value;
            }
            else // Admin
            {
                if (!learnerUserId.HasValue)
                    return BadRequest("learnerUserId is required for admin access to this view.");
                targetLearnerId = learnerUserId.Value;
            }

            var start = startDate ?? DateTime.Today;
            var end = endDate ?? start.AddDays(7);

            var sessions = await _sessionService.GetLearnerTimetableAsync(targetLearnerId, start, end);
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.LearnerUserId = targetLearnerId;
            return View(sessions);
        }

        // GET: /Session/MyChildrensTimetable?startDate=...&endDate=...
        [HttpGet]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> MyChildrensTimetable(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? start.AddDays(7);

            var model = await _sessionService.GetGuardianTimetableAsync(CurrentUserId, start, end);
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            return View(model);
        }
    }
}