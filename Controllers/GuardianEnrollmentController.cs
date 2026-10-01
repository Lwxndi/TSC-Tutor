using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.EnrollmentServices;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Parent")]
    public class GuardianEnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly Tutor_ManagerDatabaseContext _context;

        public GuardianEnrollmentController(IEnrollmentService enrollmentService, Tutor_ManagerDatabaseContext context)
        {
            _enrollmentService = enrollmentService;
            _context = context;
        }

        private int GuardianUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentService.GetForGuardianAsync(GuardianUserId);
            // NEW — the "Request Offering Change" modal used a raw numeric Offering ID
            // input; a Guardian shouldn't need to know an internal ID. Populated the same
            // way Request() already does.
            ViewBag.AvailableOfferings = await GetOfferingDropdownAsync();
            return View(enrollments);
        }

        [HttpGet]
        public async Task<IActionResult> Request()
        {
            ViewBag.AvailableLearners = await GetOwnLearnerDropdownAsync();
            ViewBag.Offerings = await GetOfferingsForRequestAsync();
            ViewBag.RegisteredSubjectPairs = await GetRegisteredSubjectPairsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(int learnerUserId, List<int>? selectedOfferingIds, Dictionary<int, DeliveryMode>? hybridDeliveryModes)
        {
            if (selectedOfferingIds == null || !selectedOfferingIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one offering.");
                ViewBag.AvailableLearners = await GetOwnLearnerDropdownAsync();
                ViewBag.Offerings = await GetOfferingsForRequestAsync();
                ViewBag.RegisteredSubjectPairs = await GetRegisteredSubjectPairsAsync();
                return View();
            }

            hybridDeliveryModes ??= new Dictionary<int, DeliveryMode>();

            // Spec §11.6 — Physical/Remote have exactly one valid DeliveryMode; this is
            // resolved server-side from the Offering itself, never taken from the client,
            // so there is nothing to tamper with for those two cases. Only Hybrid genuinely
            // needs the guardian's choice.
            var offerings = await _context.Offerings
                .Where(o => selectedOfferingIds.Contains(o.OfferingId))
                .Include(o => o.Subject)
                .ToListAsync();

            var successes = new List<string>();
            var failures = new List<string>();

            foreach (var offeringId in selectedOfferingIds)
            {
                var offering = offerings.FirstOrDefault(o => o.OfferingId == offeringId);
                if (offering == null)
                {
                    failures.Add($"Offering #{offeringId}: not found.");
                    continue;
                }

                DeliveryMode deliveryMode;
                switch (offering.DeliveryMethod)
                {
                    case DeliveryMethod.Physical:
                        deliveryMode = DeliveryMode.InPerson;
                        break;
                    case DeliveryMethod.Remote:
                        deliveryMode = DeliveryMode.Remote;
                        break;
                    case DeliveryMethod.Hybrid:
                        if (!hybridDeliveryModes.TryGetValue(offeringId, out deliveryMode))
                        {
                            failures.Add($"{offering.Subject.SubjectName}: please choose In-Person or Remote for this hybrid offering.");
                            continue;
                        }
                        break;
                    default:
                        failures.Add($"{offering.Subject.SubjectName}: unrecognized delivery method.");
                        continue;
                }

                var result = await _enrollmentService.RequestEnrollmentAsync(
                    learnerUserId, offeringId, deliveryMode, GuardianUserId);

                if (result.Succeeded)
                    successes.Add(offering.Subject.SubjectName);
                else
                    failures.Add($"{offering.Subject.SubjectName}: {result.ErrorMessage}");
            }

            // Nothing-lost principle — every offering's outcome is reported, whether the
            // whole batch succeeded or only part of it did. A partial failure (e.g. one
            // subject already has a pending request) never silently drops the others.
            if (successes.Any())
                TempData["SuccessMessage"] = $"Requested: {string.Join(", ", successes)}.";
            if (failures.Any())
                TempData["ErrorMessage"] = string.Join(" | ", failures);

            return RedirectToAction(nameof(Index));
        }

        // NEW — flat (OfferingId, SubjectId, SubjectName, Grade, TutorName, DeliveryMethod)
        // rows for the Request view's JS to group by subject and render per-offering
        // controls (fixed label for Physical/Remote, a real choice for Hybrid).
        private async Task<List<OfferingRequestRow>> GetOfferingsForRequestAsync()
        {
            return await _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Where(o => o.IsActive)
                .OrderBy(o => o.Subject.SubjectName)
                .Select(o => new OfferingRequestRow
                {
                    OfferingId = o.OfferingId,
                    SubjectId = o.SubjectId,
                    SubjectName = o.Subject.SubjectName,
                    Grade = o.Grade,
                    TutorName = o.Tutor.User.FirstName + " " + o.Tutor.User.LastName,
                    DeliveryMethod = o.DeliveryMethod
                })
                .ToListAsync();
        }

        // NEW — which subjects each of this guardian's linked learners registered
        // interest in at signup (LearnerSubject — spec §1.3: interest, not a commitment).
        // Used only to decide which subject groups start expanded; never used to filter
        // out any offering entirely.
        private async Task<List<(int LearnerUserId, int SubjectId)>> GetRegisteredSubjectPairsAsync()
        {
            var linkedLearnerIds = await _context.LearnerGuardians
                .Where(lg => lg.ParentUserId == GuardianUserId)
                .Select(lg => lg.LearnerUserId)
                .ToListAsync();

            return await _context.LearnerSubjects
                .Where(ls => linkedLearnerIds.Contains(ls.LearnerUserId))
                .Select(ls => new ValueTuple<int, int>(ls.LearnerUserId, ls.SubjectId))
                .ToListAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestChange(int enrollmentId, int requestedOfferingId)
        {
            var result = await _enrollmentService.RequestChangeAsync(
                enrollmentId, requestedOfferingId, GuardianUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? "Change request submitted, awaiting admin review."
                : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestWithdrawal(int enrollmentId, string reason)
        {
            var result = await _enrollmentService.RequestWithdrawalAsync(
                enrollmentId, reason, GuardianUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? "Withdrawal request submitted, awaiting admin action."
                : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        // NEW — spec §7.4: a Guardian can cancel their own Pending/Waitlisted request
        // outright, without waiting on Admin. Previously nothing called this even though
        // GuardianCancelled already existed as a reason code.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(int enrollmentId)
        {
            var result = await _enrollmentService.CancelRequestAsync(enrollmentId, GuardianUserId);

            TempData["SuccessMessage"] = result.Succeeded ? "Request cancelled." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        // ASSUMPTION: LearnerGuardian.ParentUserId is the FK back to this Guardian —
        // confirmed against the real LearnerGuardian.cs earlier in this build.
        private async Task<List<SelectListItem>> GetOwnLearnerDropdownAsync()
        {
            return await _context.LearnerGuardians
                .Where(lg => lg.ParentUserId == GuardianUserId)
                .Include(lg => lg.Learner).ThenInclude(l => l.User)
                .OrderBy(lg => lg.Learner.User.FirstName)
                .Select(lg => new SelectListItem
                {
                    Value = lg.LearnerUserId.ToString(),
                    Text = lg.Learner.User.FirstName + " " + lg.Learner.User.LastName
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetOfferingDropdownAsync()
        {
            return await _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Where(o => o.IsActive)
                .OrderBy(o => o.Subject.SubjectName)
                .Select(o => new SelectListItem
                {
                    Value = o.OfferingId.ToString(),
                    Text = o.Subject.SubjectName + " — Grade " + o.Grade + " — "
                        + o.Tutor.User.FirstName + " " + o.Tutor.User.LastName
                        + " (" + o.DeliveryMethod + ")"
                })
                .ToListAsync();
        }
    }

    // NEW — plain data row for the rebuilt Request view (multi-select offerings grouped
    // by subject). Not a SelectListItem, since the view needs SubjectId/DeliveryMethod
    // as real structured data for its JS grouping/per-offering controls, not just a
    // flattened display string.
    public class OfferingRequestRow
    {
        public int OfferingId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public DeliveryMethod DeliveryMethod { get; set; }
    }
}