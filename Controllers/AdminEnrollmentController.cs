using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.EnrollmentServices;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminEnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly Tutor_ManagerDatabaseContext _context;

        public AdminEnrollmentController(IEnrollmentService enrollmentService, Tutor_ManagerDatabaseContext context)
        {
            _enrollmentService = enrollmentService;
            _context = context;
        }

        private int AdminUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pending = await _enrollmentService.GetPendingForAdminAsync();
            return View(pending);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var enrollment = await _enrollmentService.GetByIdAsync(id);
            if (enrollment == null)
                return NotFound();

            // NEW — warn-don't-block info (grade mismatch, same-subject, capacity,
            // prior redirect count) so Admin sees this BEFORE deciding, not only on Create.
            ViewBag.Warnings = await _enrollmentService.GetWarningsAsync(id);
            ViewBag.Occupancy = await _enrollmentService.GetOfferingOccupancyAsync(enrollment.OfferingId);

            ViewBag.AvailableOfferings = await GetOfferingDropdownAsync();
            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _enrollmentService.ConfirmAsync(id, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Enrollment confirmed." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            if (result.Succeeded && result.Warnings.Any())
                TempData["WarningMessages"] = string.Join(" | ", result.Warnings);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Waitlist(int id)
        {
            var result = await _enrollmentService.WaitlistAsync(id, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Enrollment waitlisted." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, RejectionReasonCode reasonCode, string? note)
        {
            var result = await _enrollmentService.RejectAsync(id, reasonCode, note, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Enrollment rejected." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reconsider(int id)
        {
            var result = await _enrollmentService.ReconsiderAsync(id, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Enrollment moved back to Pending." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Redirect(int id, int newOfferingId, string? reason)
        {
            var result = await _enrollmentService.RedirectAsync(id, newOfferingId, reason, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Enrollment redirected." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            if (result.Succeeded && result.Warnings.Any())
                TempData["WarningMessages"] = string.Join(" | ", result.Warnings);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequestedChange(int id)
        {
            var result = await _enrollmentService.ApproveRequestedChangeAsync(id, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Requested change approved." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            if (result.Succeeded && result.Warnings.Any())
                TempData["WarningMessages"] = string.Join(" | ", result.Warnings);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyRequestedChange(int id)
        {
            var result = await _enrollmentService.DenyRequestedChangeAsync(id, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Requested change denied." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        // NEW — without this, a Guardian's withdrawal request could only ever end in
        // Withdraw; there was no way to decline an unwanted request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyWithdrawalRequest(int id)
        {
            var result = await _enrollmentService.DenyWithdrawalRequestAsync(id, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Withdrawal request denied." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id, WithdrawalReasonCode reasonCode, string? note)
        {
            var result = await _enrollmentService.WithdrawAsync(id, reasonCode, note, AdminUserId);
            TempData["SuccessMessage"] = result.Succeeded ? "Learner withdrawn." : null;
            TempData["ErrorMessage"] = result.Succeeded ? null : result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.AvailableLearners = await GetLearnerDropdownAsync();
            ViewBag.AvailableOfferings = await GetOfferingDropdownAsync();
            // NEW — Admin must pick a guardian to bill explicitly (no "primary guardian"
            // concept exists in this system). Flat list of all (Learner, Guardian) pairs,
            // filtered client-side to the selected learner via data-learner-id, avoiding
            // an AJAX round trip for now.
            ViewBag.LearnerGuardianPairs = await GetLearnerGuardianPairsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int learnerUserId, int offeringId, int guardianUserId, DeliveryMode deliveryMode)
        {
            var result = await _enrollmentService.CreateActiveEnrollmentAsync(
                learnerUserId, offeringId, deliveryMode, guardianUserId, AdminUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                ViewBag.AvailableLearners = await GetLearnerDropdownAsync();
                ViewBag.AvailableOfferings = await GetOfferingDropdownAsync();
                ViewBag.LearnerGuardianPairs = await GetLearnerGuardianPairsAsync();
                return View();
            }

            TempData["SuccessMessage"] = "Learner enrolled directly as Active.";
            if (result.Warnings.Any())
                TempData["WarningMessages"] = string.Join(" | ", result.Warnings);

            return RedirectToAction(nameof(Details), new { id = result.EnrollmentId });
        }

        // NEW — (LearnerUserId, GuardianUserId, GuardianName) triples for the Create
        // view's JS to filter into a per-learner guardian dropdown.
        private async Task<List<(int LearnerUserId, int GuardianUserId, string GuardianName)>> GetLearnerGuardianPairsAsync()
        {
            return await _context.LearnerGuardians
                .Include(lg => lg.Parent).ThenInclude(p => p.User)
                .Select(lg => new ValueTuple<int, int, string>(
                    lg.LearnerUserId,
                    lg.ParentUserId,
                    lg.Parent.User.FirstName + " " + lg.Parent.User.LastName))
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetLearnerDropdownAsync()
        {
            return await _context.Learners
                .Include(l => l.User)
                .OrderBy(l => l.User.FirstName)
                .Select(l => new SelectListItem
                {
                    Value = l.UserId.ToString(),
                    Text = l.User.FirstName + " " + l.User.LastName
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
}