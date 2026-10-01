using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.PaymentServices;
using Tutor_Manager.Services.EnrollmentServices;

namespace Tutor_Manager.Services.EnrollmentServices
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly ILogger<EnrollmentService> _logger;
        private readonly IPaymentService _paymentService;

        public EnrollmentService(Tutor_ManagerDatabaseContext context, ILogger<EnrollmentService> logger, IPaymentService paymentService)
        {
            _context = context;
            _logger = logger;
            _paymentService = paymentService;
        }

        // ============================================================
        // GUARDIAN ACTIONS
        // ============================================================

        public async Task<EnrollmentResult> RequestEnrollmentAsync(int learnerUserId, int offeringId, DeliveryMode deliveryMode, int guardianUserId)
        {
            if (!await IsGuardianLinkedAsync(learnerUserId, guardianUserId))
                return EnrollmentResult.Failure("You are not linked to this learner.");

            var offering = await _context.Offerings
                .FirstOrDefaultAsync(o => o.OfferingId == offeringId && o.IsActive);
            if (offering == null)
                return EnrollmentResult.Failure("Offering not found.");

            var deliveryModeError = ValidateDeliveryMode(offering, deliveryMode);
            if (deliveryModeError != null)
                return EnrollmentResult.Failure(deliveryModeError);

            var duplicateError = await CheckNonFinalDuplicateAsync(learnerUserId, offeringId);
            if (duplicateError != null)
                return EnrollmentResult.Failure(duplicateError);

            var warnings = await CollectWarningsAsync(learnerUserId, offering);

            var enrollment = new Enrollment
            {
                LearnerUserId = learnerUserId,
                OfferingId = offeringId,
                Status = EnrollmentStatus.Pending,
                InitiatedByType = InitiatedByType.Guardian,
                InitiatedByUserId = guardianUserId,
                RequestedAt = DateTime.UtcNow,
                DeliveryMode = deliveryMode
            };

            _context.Enrollments.Add(enrollment);
            var saveError = await TrySaveAsync();
            if (saveError != null)
                return EnrollmentResult.Failure(saveError);

            return EnrollmentResult.Success(enrollment.EnrollmentId, warnings);
        }

        public async Task<EnrollmentResult> RequestChangeAsync(int enrollmentId, int requestedOfferingId, int guardianUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (!await IsGuardianLinkedAsync(enrollment.LearnerUserId, guardianUserId))
                return EnrollmentResult.Failure("You are not linked to this learner.");

            // Spec §10.6 — valid while Pending, Waitlisted, or Active.
            if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted
                or EnrollmentStatus.Active))
                return EnrollmentResult.Failure("This enrollment cannot be changed in its current state.");

            if (requestedOfferingId == enrollment.OfferingId)
                return EnrollmentResult.Failure("The enrollment is already in that offering.");

            var requestedOffering = await _context.Offerings
                .FirstOrDefaultAsync(o => o.OfferingId == requestedOfferingId && o.IsActive);
            if (requestedOffering == null)
                return EnrollmentResult.Failure("Requested offering not found.");

            var deliveryModeError = ValidateDeliveryMode(requestedOffering, enrollment.DeliveryMode);
            if (deliveryModeError != null)
                return EnrollmentResult.Failure(deliveryModeError);

            // Early, friendly version of the check RedirectInternalAsync repeats at approval time.
            var duplicateError = await CheckNonFinalDuplicateAsync(
                enrollment.LearnerUserId, requestedOfferingId, excludeEnrollmentId: enrollmentId);
            if (duplicateError != null)
                return EnrollmentResult.Failure(duplicateError);

            // Last action wins if more than one linked guardian acts (spec §16.1).
            enrollment.RequestedOfferingId = requestedOfferingId;
            enrollment.RequestedChangeByUserId = guardianUserId;
            await _context.SaveChangesAsync();

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        public async Task<EnrollmentResult> RequestWithdrawalAsync(int enrollmentId, string reason, int guardianUserId)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return EnrollmentResult.Failure("Please give a reason for the withdrawal request.");

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (!await IsGuardianLinkedAsync(enrollment.LearnerUserId, guardianUserId))
                return EnrollmentResult.Failure("You are not linked to this learner.");

            // Pending/Waitlisted requests are stopped with CancelRequestAsync instead.
            if (enrollment.Status is not (EnrollmentStatus.Active or EnrollmentStatus.Suspended))
                return EnrollmentResult.Failure(
                    "Only active or suspended enrollments can be withdrawn. To stop a pending request, cancel it instead.");

            // Request only — does not itself change Status. Admin finalizes via WithdrawAsync.
            enrollment.WithdrawalRequestedAt = DateTime.UtcNow;
            enrollment.WithdrawalRequestedReason = reason.Trim();
            await _context.SaveChangesAsync();

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        // Spec §7.4 — a Guardian cancelling their own Pending/Waitlisted request lands in
        // Rejected with reason code GuardianCancelled (no admin involved).
        //public async Task<EnrollmentResult> CancelRequestAsync(int enrollmentId, int guardianUserId)
        //{
        //    var enrollment = await _context.Enrollments
        //        .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        //    if (enrollment == null)
        //        return EnrollmentResult.Failure("Enrollment not found.");

        //    if (!await IsGuardianLinkedAsync(enrollment.LearnerUserId, guardianUserId))
        //        return EnrollmentResult.Failure("You are not linked to this learner.");

        //    if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted))
        //        return EnrollmentResult.Failure("Only pending or waitlisted requests can be cancelled.");

        //    enrollment.Status = EnrollmentStatus.Rejected;
        //    enrollment.RejectionReasonCode = RejectionReasonCode.GuardianCancelled;
        //    enrollment.RejectionReasonNote = "Cancelled by guardian.";
        //    enrollment.DecidedAt = DateTime.UtcNow;
        //    enrollment.DecidedByAdminUserId = null;
        //    enrollment.RequestedOfferingId = null;
        //    enrollment.RequestedChangeByUserId = null;
        //    await _context.SaveChangesAsync();

        //    return EnrollmentResult.Success(enrollment.EnrollmentId);
        //}

        // ============================================================
        // ADMIN ACTIONS
        // ============================================================

        //public async Task<EnrollmentResult> CreateActiveEnrollmentAsync(int learnerUserId, int offeringId, DeliveryMode deliveryMode, int guardianUserId, int adminUserId)
        //{
        //    if (!await _context.Learners.AnyAsync(l => l.UserId == learnerUserId))
        //        return EnrollmentResult.Failure("Learner not found.");

        //    // Spec §18.1/§29.1 — Guardian is billed for every invoice, including one
        //    // generated from an Admin-direct-created enrollment. There is no "primary
        //    // guardian" concept in this system, so Admin must pick one explicitly here
        //    // rather than the system guessing which linked guardian to bill.
        //    if (!await IsGuardianLinkedAsync(learnerUserId, guardianUserId))
        //        return EnrollmentResult.Failure("Selected guardian is not linked to this learner.");

        //    var offering = await _context.Offerings
        //        .FirstOrDefaultAsync(o => o.OfferingId == offeringId && o.IsActive);
        //    if (offering == null)
        //        return EnrollmentResult.Failure("Offering not found.");

        //    var deliveryModeError = ValidateDeliveryMode(offering, deliveryMode);
        //    if (deliveryModeError != null)
        //        return EnrollmentResult.Failure(deliveryModeError);

        //    var duplicateError = await CheckNonFinalDuplicateAsync(learnerUserId, offeringId);
        //    if (duplicateError != null)
        //        return EnrollmentResult.Failure(duplicateError);

        //    var warnings = await CollectWarningsAsync(learnerUserId, offering);
        //    warnings.AddRange(await CapacityWarningAsync(offering));

        //    var enrollment = new Enrollment
        //    {
        //        LearnerUserId = learnerUserId,
        //        OfferingId = offeringId,
        //        Status = EnrollmentStatus.Active,
        //        InitiatedByType = InitiatedByType.Admin,
        //        InitiatedByUserId = adminUserId,
        //        RequestedAt = DateTime.UtcNow,
        //        DecidedAt = DateTime.UtcNow,
        //        DecidedByAdminUserId = adminUserId,
        //        DeliveryMode = deliveryMode
        //    };

        //    _context.Enrollments.Add(enrollment);
        //    var saveError = await TrySaveAsync();
        //    if (saveError != null)
        //        return EnrollmentResult.Failure(saveError);

        //    // Spec §19.1 — all paths to Active trigger Invoice generation. A billing
        //    // failure here does NOT undo or fail the enrollment itself (§8.2 — access is
        //    // never gated on payment) — it surfaces as a warning for Admin to notice and
        //    // follow up on manually.
        //    var invoiceResult = await _paymentService.GenerateInvoiceForActiveEnrollmentAsync(enrollment.EnrollmentId, guardianUserId);
        //    if (!invoiceResult.Succeeded)
        //    {
        //        _logger.LogError("Invoice generation failed for enrollment {EnrollmentId}: {Error}", enrollment.EnrollmentId, invoiceResult.ErrorMessage);
        //        warnings.Add($"Enrollment created, but invoice generation failed: {invoiceResult.ErrorMessage}");
        //    }

        //    return EnrollmentResult.Success(enrollment.EnrollmentId, warnings);
        //}

        //public async Task<EnrollmentResult> ConfirmAsync(int enrollmentId, int adminUserId)
        //{
        //    var enrollment = await _context.Enrollments
        //        .Include(e => e.Offering)
        //        .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        //    if (enrollment == null)
        //        return EnrollmentResult.Failure("Enrollment not found.");

        //    if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted))
        //        return EnrollmentResult.Failure("Only Pending or Waitlisted enrollments can be confirmed.");

        //    // The Offering's DeliveryMethod may have been edited since the request was made.
        //    var deliveryModeError = ValidateDeliveryMode(enrollment.Offering, enrollment.DeliveryMode);
        //    if (deliveryModeError != null)
        //        return EnrollmentResult.Failure(deliveryModeError +
        //            " The offering may have changed since this request was made — redirect the enrollment or ask the guardian to re-request.");

        //    // Warn-don't-block: capacity, grade and same-subject issues are surfaced, Admin overrides.
        //    var warnings = await BuildWarningsAsync(enrollment);

        //    enrollment.Status = EnrollmentStatus.Active;
        //    enrollment.DecidedAt = DateTime.UtcNow;
        //    enrollment.DecidedByAdminUserId = adminUserId;
        //    await _context.SaveChangesAsync();

        //    // Spec §19.1 — all paths to Active trigger Invoice generation, EXCEPT
        //    // Suspended -> Active (§19.2, handled separately by
        //    // RestoreFromSuspensionAsync, which must NOT call this).
        //    //
        //    // guardianUserId here is enrollment.InitiatedByUserId, which is safe ONLY
        //    // because the sole path that produces a Pending/Waitlisted enrollment
        //    // (RequestEnrollmentAsync) always sets InitiatedByType = Guardian. If that
        //    // ever changes, this resolution needs revisiting.
        //    var invoiceResult = await _paymentService.GenerateInvoiceForActiveEnrollmentAsync(enrollment.EnrollmentId, enrollment.InitiatedByUserId);
        //    if (!invoiceResult.Succeeded)
        //    {
        //        _logger.LogError("Invoice generation failed for enrollment {EnrollmentId}: {Error}", enrollment.EnrollmentId, invoiceResult.ErrorMessage);
        //        warnings.Add($"Enrollment confirmed, but invoice generation failed: {invoiceResult.ErrorMessage}");
        //    }

        //    return EnrollmentResult.Success(enrollment.EnrollmentId, warnings);
        //}

        public async Task<EnrollmentResult> WaitlistAsync(int enrollmentId, int adminUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status != EnrollmentStatus.Pending)
                return EnrollmentResult.Failure("Only Pending enrollments can be waitlisted.");

            enrollment.Status = EnrollmentStatus.Waitlisted;
            enrollment.DecidedAt = DateTime.UtcNow;
            enrollment.DecidedByAdminUserId = adminUserId;
            await _context.SaveChangesAsync();

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        public async Task<EnrollmentResult> RejectAsync(int enrollmentId, RejectionReasonCode reasonCode, string? note, int adminUserId)
        {
            // GuardianCancelled is only ever set by CancelRequestAsync.
            if (reasonCode == RejectionReasonCode.GuardianCancelled)
                return EnrollmentResult.Failure(
                    "GuardianCancelled is set via the guardian cancellation path, not admin rejection.");

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted))
                return EnrollmentResult.Failure("Only Pending or Waitlisted enrollments can be rejected.");

            enrollment.Status = EnrollmentStatus.Rejected;
            enrollment.RejectionReasonCode = reasonCode;
            enrollment.RejectionReasonNote = note;
            enrollment.DecidedAt = DateTime.UtcNow;
            enrollment.DecidedByAdminUserId = adminUserId;
            enrollment.RequestedOfferingId = null;
            enrollment.RequestedChangeByUserId = null;
            await _context.SaveChangesAsync();

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        public async Task<EnrollmentResult> ReconsiderAsync(int enrollmentId, int adminUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status != EnrollmentStatus.Rejected)
                return EnrollmentResult.Failure("Only Rejected enrollments can be reconsidered.");

            // A request the guardian withdrew themselves isn't the admin's to revive.
            // The guardian can simply submit a new request (Rejected doesn't block that).
            if (enrollment.RejectionReasonCode == RejectionReasonCode.GuardianCancelled)
                return EnrollmentResult.Failure(
                    "This request was cancelled by the guardian. Ask them to submit a new request.");

            // Re-check the uniqueness rule — someone else may have taken this slot meanwhile.
            var duplicateError = await CheckNonFinalDuplicateAsync(
                enrollment.LearnerUserId, enrollment.OfferingId, excludeEnrollmentId: enrollmentId);
            if (duplicateError != null)
                return EnrollmentResult.Failure(duplicateError);

            enrollment.Status = EnrollmentStatus.Pending;
            enrollment.RejectionReasonCode = null;
            enrollment.RejectionReasonNote = null;
            enrollment.DecidedAt = null;
            enrollment.DecidedByAdminUserId = null;

            var saveError = await TrySaveAsync();
            if (saveError != null)
                return EnrollmentResult.Failure(saveError);

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        public async Task<EnrollmentResult> RedirectAsync(int enrollmentId, int newOfferingId, string? reason, int adminUserId)
        {
            return await RedirectInternalAsync(enrollmentId, newOfferingId, reason,
                InitiatedByType.Admin, adminUserId);
        }

        public async Task<EnrollmentResult> ApproveRequestedChangeAsync(int enrollmentId, int adminUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.RequestedOfferingId == null)
                return EnrollmentResult.Failure("No pending offering change request on this enrollment.");

            // The history row must name who actually initiated the change (the Guardian who
            // filed the request), not the admin who approved it. The approver goes in Reason.
            var initiatorType = enrollment.RequestedChangeByUserId.HasValue
                ? InitiatedByType.Guardian
                : InitiatedByType.Admin;
            var initiatorUserId = enrollment.RequestedChangeByUserId ?? adminUserId;

            // RedirectInternalAsync clears RequestedOfferingId in the same save as the redirect.
            return await RedirectInternalAsync(
                enrollmentId,
                enrollment.RequestedOfferingId.Value,
                $"Guardian-requested change, approved by admin #{adminUserId}",
                initiatorType,
                initiatorUserId);
        }

        public async Task<EnrollmentResult> DenyRequestedChangeAsync(int enrollmentId, int adminUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.RequestedOfferingId == null)
                return EnrollmentResult.Failure("No pending offering change request on this enrollment.");

            enrollment.RequestedOfferingId = null;
            enrollment.RequestedChangeByUserId = null;
            await _context.SaveChangesAsync();

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        // Without this, a Guardian's withdrawal request could only ever be answered by
        // actually withdrawing — an unwanted request would sit in the admin queue forever.
        public async Task<EnrollmentResult> DenyWithdrawalRequestAsync(int enrollmentId, int adminUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.WithdrawalRequestedAt == null)
                return EnrollmentResult.Failure("No pending withdrawal request on this enrollment.");

            enrollment.WithdrawalRequestedAt = null;
            enrollment.WithdrawalRequestedReason = null;
            await _context.SaveChangesAsync();

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        //public async Task<EnrollmentResult> WithdrawAsync(int enrollmentId, WithdrawalReasonCode reasonCode, string? note, int adminUserId)
        //{
        //    var enrollment = await _context.Enrollments
        //        .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        //    if (enrollment == null)
        //        return EnrollmentResult.Failure("Enrollment not found.");

        //    // Spec §21.6 — Admin may withdraw an Active or a Suspended enrollment.
        //    if (enrollment.Status is not (EnrollmentStatus.Active or EnrollmentStatus.Suspended))
        //        return EnrollmentResult.Failure("Only Active or Suspended enrollments can be withdrawn.");

        //    enrollment.Status = EnrollmentStatus.Withdrawn;
        //    enrollment.WithdrawnAt = DateTime.UtcNow;
        //    enrollment.WithdrawalReasonCode = reasonCode;
        //    enrollment.WithdrawalReasonNote = note;
        //    enrollment.WithdrawnByAdminUserId = adminUserId;
        //    enrollment.RequestedOfferingId = null;
        //    enrollment.RequestedChangeByUserId = null;
        //    await _context.SaveChangesAsync();

        //    return EnrollmentResult.Success(enrollment.EnrollmentId);
        //}

        // ============================================================
        // SYSTEM-TRIGGERED (Phase B hooks)
        // ============================================================

        public async Task<EnrollmentResult> SuspendForNonPaymentAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status != EnrollmentStatus.Active)
                return EnrollmentResult.Failure("Only Active enrollments can be suspended.");

            enrollment.Status = EnrollmentStatus.Suspended;
            enrollment.SuspendedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        public async Task<EnrollmentResult> RestoreFromSuspensionAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status != EnrollmentStatus.Suspended)
                return EnrollmentResult.Failure("Only Suspended enrollments can be restored.");

            enrollment.Status = EnrollmentStatus.Active;
            enrollment.SuspendedAt = null;
            await _context.SaveChangesAsync();

            // Spec §19.2 — must NOT generate a new Invoice; this resolves an existing one.

            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        // ============================================================
        // READS
        // ============================================================

        // Every view reads Learner.User, Offering.Subject and Offering.Tutor.User, so all
        // list/detail reads load them. Without these ThenIncludes those properties are null
        // (no lazy loading) and the views throw NullReferenceException.
        private IQueryable<Enrollment> EnrollmentsWithDisplayData() =>
            _context.Enrollments
                .Include(e => e.Learner).ThenInclude(l => l.User)
                .Include(e => e.Offering).ThenInclude(o => o.Subject)
                .Include(e => e.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(e => e.RequestedOffering).ThenInclude(o => o!.Subject);

        public async Task<Enrollment?> GetByIdAsync(int enrollmentId) =>
            await EnrollmentsWithDisplayData()
                .Include(e => e.RedirectHistory.OrderByDescending(h => h.RedirectedAt))
                    .ThenInclude(h => h.PreviousOffering).ThenInclude(o => o.Subject)
                .Include(e => e.RedirectHistory.OrderByDescending(h => h.RedirectedAt))
                    .ThenInclude(h => h.NewOffering).ThenInclude(o => o.Subject)
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

        // Admin action queue: new requests, plus Guardian-filed change/withdrawal
        // requests on enrollments that are already live (which would otherwise be invisible).
        public async Task<List<Enrollment>> GetPendingForAdminAsync() =>
            await EnrollmentsWithDisplayData()
                .Where(e =>
                    e.Status == EnrollmentStatus.Pending ||
                    e.Status == EnrollmentStatus.Waitlisted ||
                    ((e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Suspended) &&
                     (e.RequestedOfferingId != null || e.WithdrawalRequestedAt != null)))
                .OrderBy(e => e.RequestedAt)
                .ToListAsync();

        public async Task<List<Enrollment>> GetForLearnerAsync(int learnerUserId) =>
            await EnrollmentsWithDisplayData()
                .Where(e => e.LearnerUserId == learnerUserId)
                .OrderByDescending(e => e.RequestedAt)
                .ToListAsync();

        public async Task<List<Enrollment>> GetForGuardianAsync(int guardianUserId)
        {
            var linkedLearnerIds = await _context.LearnerGuardians
                .Where(lg => lg.ParentUserId == guardianUserId)
                .Select(lg => lg.LearnerUserId)
                .ToListAsync();

            return await EnrollmentsWithDisplayData()
                .Where(e => linkedLearnerIds.Contains(e.LearnerUserId))
                .OrderByDescending(e => e.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<Enrollment>> GetActiveRosterForOfferingAsync(int offeringId) =>
            await _context.Enrollments
                .Where(e => e.OfferingId == offeringId && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Learner).ThenInclude(l => l.User)
                .OrderBy(e => e.Learner.User.FirstName)
                .ToListAsync();

        // Warnings for an existing enrollment, shown on the Admin Details page BEFORE
        // the admin decides (spec §11.1-11.3, §9, §10.8). Warn, never block.
        public async Task<List<string>> GetWarningsAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Offering)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return new List<string>();

            return await BuildWarningsAsync(enrollment);
        }

        public async Task<(int Taken, int Capacity)> GetOfferingOccupancyAsync(int offeringId)
        {
            // ASSUMPTION: Offering.Capacity is a non-nullable int. If it's int?, coalesce here.
            var capacity = await _context.Offerings
                .Where(o => o.OfferingId == offeringId)
                .Select(o => o.Capacity)
                .FirstOrDefaultAsync();

            return (await CountSeatsTakenAsync(offeringId), capacity);
        }

        // ============================================================
        // ACCESS GATING (spec §14) — only Active grants access
        // ============================================================

        public async Task<bool> HasActiveAccessAsync(int learnerUserId, int offeringId) =>
            await _context.Enrollments.AnyAsync(e =>
                e.LearnerUserId == learnerUserId &&
                e.OfferingId == offeringId &&
                e.Status == EnrollmentStatus.Active);

        public async Task<bool> IsActiveInSubjectAsync(int learnerUserId, int subjectId) =>
            await _context.Enrollments.AnyAsync(e =>
                e.LearnerUserId == learnerUserId &&
                e.Status == EnrollmentStatus.Active &&
                e.Offering.SubjectId == subjectId);

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        private async Task<bool> IsGuardianLinkedAsync(int learnerUserId, int guardianUserId) =>
            await _context.LearnerGuardians
                .AnyAsync(lg => lg.LearnerUserId == learnerUserId && lg.ParentUserId == guardianUserId);

        private async Task<string?> TrySaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return null;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Enrollment save failed");
                return "The change could not be saved — it may conflict with an existing enrollment. Please refresh and try again.";
            }
        }

        // Spec §11.6 — the one deliberate hard block. This is also the explicit
        // Physical <-> InPerson mapping (the two enums use different names for it).
        private static string? ValidateDeliveryMode(Offering offering, DeliveryMode requestedMode)
        {
            return offering.DeliveryMethod switch
            {
                DeliveryMethod.Physical when requestedMode != DeliveryMode.InPerson =>
                    "This offering is in-person only; remote is not available.",
                DeliveryMethod.Remote when requestedMode != DeliveryMode.Remote =>
                    "This offering is remote only; in-person is not available.",
                _ => null // Hybrid allows either
            };
        }

        // Spec §12 — mirrors the DB filtered unique index so a clear message comes back
        // before a raw constraint violation.
        private async Task<string?> CheckNonFinalDuplicateAsync(int learnerUserId, int offeringId, int? excludeEnrollmentId = null)
        {
            var nonFinalStatuses = new[]
            {
                EnrollmentStatus.Pending, EnrollmentStatus.Waitlisted,
                EnrollmentStatus.Active, EnrollmentStatus.Suspended,
                EnrollmentStatus.AwaitingPayment
            };

            var exists = await _context.Enrollments.AnyAsync(e =>
                e.LearnerUserId == learnerUserId &&
                e.OfferingId == offeringId &&
                nonFinalStatuses.Contains(e.Status) &&
                (excludeEnrollmentId == null || e.EnrollmentId != excludeEnrollmentId));

            return exists
                ? "This learner already has an active or pending enrollment for this offering."
                : null;
        }

        // Seats held = Active + Suspended. Suspended still holds the seat because it can
        // restore to Active automatically on payment; freeing it would risk overfilling.
        //private async Task<int> CountSeatsTakenAsync(int offeringId) =>
        //    await _context.Enrollments.CountAsync(e =>
        //        e.OfferingId == offeringId &&
        //        (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Suspended));



        private async Task<List<string>> CapacityWarningAsync(Offering offering)
        {
            var taken = await CountSeatsTakenAsync(offering.OfferingId);
            return taken >= offering.Capacity
                ? new List<string> { $"This offering is full ({taken}/{offering.Capacity}). Consider Waitlist or Redirect." }
                : new List<string>();
        }

        // Spec §11.1-11.2 — warnings only, Admin can always override.
        private async Task<List<string>> CollectWarningsAsync(int learnerUserId, Offering offering, int? excludeEnrollmentId = null)
        {
            var warnings = new List<string>();

            // Confirmed: Learner.GradeLevel is Grade? (nullable), Offering.Grade is Grade
            // (non-nullable). A learner with no grade set yet must NOT trigger a mismatch
            // warning — HasValue guard is required or every gradeless learner false-positives.
            var learner = await _context.Learners.FirstOrDefaultAsync(l => l.UserId == learnerUserId);
            if (learner?.GradeLevel is { } learnerGrade && learnerGrade != offering.Grade)
            {
                warnings.Add($"Learner's grade ({learnerGrade}) does not match this offering's grade ({offering.Grade}).");
            }

            var hasActiveSameSubject = await _context.Enrollments.AnyAsync(e =>
                e.LearnerUserId == learnerUserId &&
                e.Status == EnrollmentStatus.Active &&
                e.Offering.SubjectId == offering.SubjectId &&
                (excludeEnrollmentId == null || e.EnrollmentId != excludeEnrollmentId));
            if (hasActiveSameSubject)
            {
                warnings.Add("Learner already has an active enrollment in another offering for this subject.");
            }

            return warnings;
        }

        // Requires enrollment.Offering to be loaded.
        private async Task<List<string>> BuildWarningsAsync(Enrollment enrollment)
        {
            var warnings = await CollectWarningsAsync(
                enrollment.LearnerUserId, enrollment.Offering, enrollment.EnrollmentId);

            // Capacity only matters while the enrollment isn't yet holding a seat.
            if (enrollment.Status is EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted)
                warnings.AddRange(await CapacityWarningAsync(enrollment.Offering));

            // Spec §10.8 — prior redirects shown as a warning when reviewing a requested change.
            if (enrollment.RequestedOfferingId != null && enrollment.RedirectCount > 0)
                warnings.Add($"This learner has already been redirected {enrollment.RedirectCount} time(s).");

            return warnings;
        }

        // Shared redirect mechanic — Admin-initiated and Guardian-requested/Admin-approved.
        // Works identically from Pending, Waitlisted, or Active (spec §10.2). Offering change,
        // history row and clearing of any pending change request all commit in ONE save.
        //private async Task<EnrollmentResult> RedirectInternalAsync(int enrollmentId, int newOfferingId, string? reason, InitiatedByType initiatedByType, int initiatedByUserId)
        //{
        //    var enrollment = await _context.Enrollments
        //        .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        //    if (enrollment == null)
        //        return EnrollmentResult.Failure("Enrollment not found.");

        //    if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted
        //        or EnrollmentStatus.Active))
        //        return EnrollmentResult.Failure("Enrollment cannot be redirected in its current state.");

        //    if (newOfferingId == enrollment.OfferingId)
        //        return EnrollmentResult.Failure("The enrollment is already in that offering.");

        //    var newOffering = await _context.Offerings
        //        .FirstOrDefaultAsync(o => o.OfferingId == newOfferingId && o.IsActive);
        //    if (newOffering == null)
        //        return EnrollmentResult.Failure("Target offering not found.");

        //    var deliveryModeError = ValidateDeliveryMode(newOffering, enrollment.DeliveryMode);
        //    if (deliveryModeError != null)
        //        return EnrollmentResult.Failure(deliveryModeError);

        //    var duplicateError = await CheckNonFinalDuplicateAsync(
        //        enrollment.LearnerUserId, newOfferingId, excludeEnrollmentId: enrollmentId);
        //    if (duplicateError != null)
        //        return EnrollmentResult.Failure(duplicateError);

        //    var warnings = await CollectWarningsAsync(enrollment.LearnerUserId, newOffering, enrollmentId);
        //    warnings.AddRange(await CapacityWarningAsync(newOffering));

        //    var previousOfferingId = enrollment.OfferingId;

        //    enrollment.OfferingId = newOfferingId;
        //    enrollment.RedirectCount += 1;

        //    // Spec §10.5 — any pending Guardian change request is cleared once Admin acts,
        //    // whether via approval or a direct redirect elsewhere.
        //    enrollment.RequestedOfferingId = null;
        //    enrollment.RequestedChangeByUserId = null;

        //    _context.EnrollmentRedirectHistories.Add(new EnrollmentRedirectHistory
        //    {
        //        EnrollmentId = enrollment.EnrollmentId,
        //        PreviousOfferingId = previousOfferingId,
        //        NewOfferingId = newOfferingId,
        //        RedirectedAt = DateTime.UtcNow,
        //        Reason = reason,
        //        InitiatedByType = initiatedByType,
        //        InitiatedByUserId = initiatedByUserId
        //    });

        //    var saveError = await TrySaveAsync();
        //    if (saveError != null)
        //        return EnrollmentResult.Failure(saveError);

        //    // Spec §20.7 — new Offering's price applies from the NEXT billing cycle only.
        //    // TODO (Phase B): tell the billing schedule about the OfferingId change without
        //    // touching any already-generated current-cycle Invoice/LineItem.

        //    return EnrollmentResult.Success(enrollment.EnrollmentId, warnings);
        //}














        public async Task<EnrollmentResult> CancelRequestAsync(int enrollmentId, int guardianUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (!await IsGuardianLinkedAsync(enrollment.LearnerUserId, guardianUserId))
                return EnrollmentResult.Failure("You are not linked to this learner.");

            if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted
                or EnrollmentStatus.AwaitingPayment))
                return EnrollmentResult.Failure("Only pending, waitlisted or awaiting-payment requests can be cancelled.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            enrollment.Status = EnrollmentStatus.Rejected;
            enrollment.RejectionReasonCode = RejectionReasonCode.GuardianCancelled;
            enrollment.RejectionReasonNote = "Cancelled by guardian.";
            enrollment.DecidedAt = DateTime.UtcNow;
            enrollment.DecidedByAdminUserId = null;
            enrollment.RequestedOfferingId = null;
            enrollment.RequestedChangeByUserId = null;
            await _context.SaveChangesAsync();

            // If it was awaiting payment: void the unpaid invoice and kill any open Stripe session.
            // (Does nothing for Pending/Waitlisted, which have no invoice.)
            await _paymentService.VoidUnpaidInvoicesForEnrollmentAsync(enrollmentId, "Cancelled by guardian before payment.");

            await tx.CommitAsync();
            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        public async Task<EnrollmentResult> CreateActiveEnrollmentAsync(int learnerUserId, int offeringId, DeliveryMode deliveryMode, int guardianUserId, int adminUserId)
        {
            if (!await _context.Learners.AnyAsync(l => l.UserId == learnerUserId))
                return EnrollmentResult.Failure("Learner not found.");

            // No "primary guardian" concept, so Admin picks who is billed.
            if (!await IsGuardianLinkedAsync(learnerUserId, guardianUserId))
                return EnrollmentResult.Failure("Selected guardian is not linked to this learner.");

            var offering = await _context.Offerings
                .FirstOrDefaultAsync(o => o.OfferingId == offeringId && o.IsActive);
            if (offering == null)
                return EnrollmentResult.Failure("Offering not found.");

            var deliveryModeError = ValidateDeliveryMode(offering, deliveryMode);
            if (deliveryModeError != null)
                return EnrollmentResult.Failure(deliveryModeError);

            var duplicateError = await CheckNonFinalDuplicateAsync(learnerUserId, offeringId);
            if (duplicateError != null)
                return EnrollmentResult.Failure(duplicateError);

            var warnings = await CollectWarningsAsync(learnerUserId, offering);
            warnings.AddRange(await CapacityWarningAsync(offering));

            // Payment-first: the learner gets NO access until the guardian pays. Holds the seat meanwhile.
            var enrollment = new Enrollment
            {
                LearnerUserId = learnerUserId,
                OfferingId = offeringId,
                Status = EnrollmentStatus.AwaitingPayment,
                InitiatedByType = InitiatedByType.Admin,
                InitiatedByUserId = adminUserId,
                RequestedAt = DateTime.UtcNow,
                DecidedAt = DateTime.UtcNow,
                DecidedByAdminUserId = adminUserId,
                DeliveryMode = deliveryMode
            };

            // Enrollment + first invoice commit together. An AwaitingPayment enrollment with no
            // invoice would be stuck forever, so if the invoice fails, nothing is created.
            await using var tx = await _context.Database.BeginTransactionAsync();

            _context.Enrollments.Add(enrollment);
            var saveError = await TrySaveAsync();
            if (saveError != null)
                return EnrollmentResult.Failure(saveError);

            var invoiceResult = await _paymentService.GenerateFirstInvoiceAsync(enrollment.EnrollmentId, guardianUserId);
            if (!invoiceResult.Succeeded)
            {
                await tx.RollbackAsync();
                _logger.LogError("First invoice generation failed for enrollment {EnrollmentId}: {Error}", enrollment.EnrollmentId, invoiceResult.ErrorMessage);
                return EnrollmentResult.Failure($"The enrollment was not created because its invoice could not be generated: {invoiceResult.ErrorMessage}");
            }

            await tx.CommitAsync();
            return EnrollmentResult.Success(enrollment.EnrollmentId, warnings);
        }

        public async Task<EnrollmentResult> ConfirmAsync(int enrollmentId, int adminUserId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Offering)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted))
                return EnrollmentResult.Failure("Only Pending or Waitlisted enrollments can be confirmed.");

            var deliveryModeError = ValidateDeliveryMode(enrollment.Offering, enrollment.DeliveryMode);
            if (deliveryModeError != null)
                return EnrollmentResult.Failure(deliveryModeError +
                    " The offering may have changed since this request was made — redirect the enrollment or ask the guardian to re-request.");

            // Warn-don't-block: capacity, grade and same-subject issues are surfaced, Admin overrides.
            var warnings = await BuildWarningsAsync(enrollment);

            // Guardian to bill = whoever requested it. Safe ONLY because the sole path producing a
            // Pending/Waitlisted enrollment (RequestEnrollmentAsync) always sets InitiatedByType = Guardian.
            await using var tx = await _context.Database.BeginTransactionAsync();

            enrollment.Status = EnrollmentStatus.AwaitingPayment; // access is granted only once the guardian pays
            enrollment.DecidedAt = DateTime.UtcNow;
            enrollment.DecidedByAdminUserId = adminUserId;
            await _context.SaveChangesAsync();

            var invoiceResult = await _paymentService.GenerateFirstInvoiceAsync(enrollment.EnrollmentId, enrollment.InitiatedByUserId);
            if (!invoiceResult.Succeeded)
            {
                await tx.RollbackAsync();
                _logger.LogError("First invoice generation failed for enrollment {EnrollmentId}: {Error}", enrollment.EnrollmentId, invoiceResult.ErrorMessage);
                return EnrollmentResult.Failure($"The enrollment was not confirmed because its invoice could not be generated: {invoiceResult.ErrorMessage}");
            }

            await tx.CommitAsync();
            return EnrollmentResult.Success(enrollment.EnrollmentId, warnings);
        }

        public async Task<EnrollmentResult> WithdrawAsync(int enrollmentId, WithdrawalReasonCode reasonCode, string? note, int adminUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status is not (EnrollmentStatus.Active or EnrollmentStatus.Suspended
                or EnrollmentStatus.AwaitingPayment))
                return EnrollmentResult.Failure("Only Active, Suspended or Awaiting Payment enrollments can be withdrawn.");

            var wasAwaitingPayment = enrollment.Status == EnrollmentStatus.AwaitingPayment;

            await using var tx = await _context.Database.BeginTransactionAsync();

            enrollment.Status = EnrollmentStatus.Withdrawn;
            enrollment.WithdrawnAt = DateTime.UtcNow;
            enrollment.WithdrawalReasonCode = reasonCode;
            enrollment.WithdrawalReasonNote = note;
            enrollment.WithdrawnByAdminUserId = adminUserId;
            enrollment.RequestedOfferingId = null;
            enrollment.RequestedChangeByUserId = null;
            await _context.SaveChangesAsync();

            // Never-paid enrollment: cancel its invoice. An Active/Suspended one keeps whatever it owes.
            if (wasAwaitingPayment)
                await _paymentService.VoidUnpaidInvoicesForEnrollmentAsync(enrollmentId, "Enrollment withdrawn before payment.");

            await tx.CommitAsync();
            return EnrollmentResult.Success(enrollment.EnrollmentId);
        }

        private async Task<EnrollmentResult> RedirectInternalAsync(int enrollmentId, int newOfferingId, string? reason, InitiatedByType initiatedByType, int initiatedByUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return EnrollmentResult.Failure("Enrollment not found.");

            if (enrollment.Status is not (EnrollmentStatus.Pending or EnrollmentStatus.Waitlisted
                or EnrollmentStatus.Active or EnrollmentStatus.AwaitingPayment))
                return EnrollmentResult.Failure("Enrollment cannot be redirected in its current state.");

            if (newOfferingId == enrollment.OfferingId)
                return EnrollmentResult.Failure("The enrollment is already in that offering.");

            var newOffering = await _context.Offerings
                .FirstOrDefaultAsync(o => o.OfferingId == newOfferingId && o.IsActive);
            if (newOffering == null)
                return EnrollmentResult.Failure("Target offering not found.");

            var deliveryModeError = ValidateDeliveryMode(newOffering, enrollment.DeliveryMode);
            if (deliveryModeError != null)
                return EnrollmentResult.Failure(deliveryModeError);

            var duplicateError = await CheckNonFinalDuplicateAsync(
                enrollment.LearnerUserId, newOfferingId, excludeEnrollmentId: enrollmentId);
            if (duplicateError != null)
                return EnrollmentResult.Failure(duplicateError);

            var warnings = await CollectWarningsAsync(enrollment.LearnerUserId, newOffering, enrollmentId);
            warnings.AddRange(await CapacityWarningAsync(newOffering));

            // Awaiting payment: the redirect and the re-issued invoice must succeed or fail together.
            var isAwaitingPayment = enrollment.Status == EnrollmentStatus.AwaitingPayment;
            await using var tx = isAwaitingPayment ? await _context.Database.BeginTransactionAsync() : null;

            var previousOfferingId = enrollment.OfferingId;

            enrollment.OfferingId = newOfferingId;
            enrollment.RedirectCount += 1;

            // Spec §10.5 — any pending Guardian change request is cleared once Admin acts.
            enrollment.RequestedOfferingId = null;
            enrollment.RequestedChangeByUserId = null;

            _context.EnrollmentRedirectHistories.Add(new EnrollmentRedirectHistory
            {
                EnrollmentId = enrollment.EnrollmentId,
                PreviousOfferingId = previousOfferingId,
                NewOfferingId = newOfferingId,
                RedirectedAt = DateTime.UtcNow,
                Reason = reason,
                InitiatedByType = initiatedByType,
                InitiatedByUserId = initiatedByUserId
            });

            var saveError = await TrySaveAsync();
            if (saveError != null)
                return EnrollmentResult.Failure(saveError);

            if (isAwaitingPayment)
            {
                // Void the unpaid invoice (and any open Stripe session) and issue a new one at the new price.
                var reissue = await _paymentService.RegenerateFirstInvoiceAsync(enrollmentId);
                if (!reissue.Succeeded)
                {
                    await tx!.RollbackAsync();
                    return EnrollmentResult.Failure($"The redirect was not applied because the invoice could not be re-issued: {reissue.ErrorMessage}");
                }
                await tx!.CommitAsync();
            }

            // Spec §20.7 — for an Active enrollment the new price applies from the NEXT billing run only:
            // GenerateDueRecurringInvoicesAsync reads the live Offering, so nothing more is needed here.

            return EnrollmentResult.Success(enrollment.EnrollmentId, warnings);
        }

        // Seats held = Active + Suspended + AwaitingPayment.
        private async Task<int> CountSeatsTakenAsync(int offeringId) =>
            await _context.Enrollments.CountAsync(e =>
                e.OfferingId == offeringId &&
                (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Suspended
                    || e.Status == EnrollmentStatus.AwaitingPayment));
    }
}