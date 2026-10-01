using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Services.EnrollmentServices
{
    public interface IEnrollmentService
    {
        // Guardian actions
        Task<EnrollmentResult> RequestEnrollmentAsync(int learnerUserId, int offeringId, DeliveryMode deliveryMode, int guardianUserId);
        Task<EnrollmentResult> RequestChangeAsync(int enrollmentId, int requestedOfferingId, int guardianUserId);
        Task<EnrollmentResult> RequestWithdrawalAsync(int enrollmentId, string reason, int guardianUserId);
        Task<EnrollmentResult> CancelRequestAsync(int enrollmentId, int guardianUserId);              // NEW

        // Admin actions
        Task<EnrollmentResult> CreateActiveEnrollmentAsync(int learnerUserId, int offeringId, DeliveryMode deliveryMode, int guardianUserId, int adminUserId);
        Task<EnrollmentResult> ConfirmAsync(int enrollmentId, int adminUserId);
        Task<EnrollmentResult> WaitlistAsync(int enrollmentId, int adminUserId);
        Task<EnrollmentResult> RejectAsync(int enrollmentId, RejectionReasonCode reasonCode, string? note, int adminUserId);
        Task<EnrollmentResult> ReconsiderAsync(int enrollmentId, int adminUserId);
        Task<EnrollmentResult> RedirectAsync(int enrollmentId, int newOfferingId, string? reason, int adminUserId);
        Task<EnrollmentResult> ApproveRequestedChangeAsync(int enrollmentId, int adminUserId);
        Task<EnrollmentResult> DenyRequestedChangeAsync(int enrollmentId, int adminUserId);
        Task<EnrollmentResult> DenyWithdrawalRequestAsync(int enrollmentId, int adminUserId);         // NEW
        Task<EnrollmentResult> WithdrawAsync(int enrollmentId, WithdrawalReasonCode reasonCode, string? note, int adminUserId);

        // System-triggered (Phase B hooks)
        Task<EnrollmentResult> SuspendForNonPaymentAsync(int enrollmentId);
        Task<EnrollmentResult> RestoreFromSuspensionAsync(int enrollmentId);

        // Reads
        Task<Enrollment?> GetByIdAsync(int enrollmentId);
        Task<List<Enrollment>> GetPendingForAdminAsync();
        Task<List<Enrollment>> GetForLearnerAsync(int learnerUserId);
        Task<List<Enrollment>> GetForGuardianAsync(int guardianUserId);
        Task<List<Enrollment>> GetActiveRosterForOfferingAsync(int offeringId);
        Task<List<string>> GetWarningsAsync(int enrollmentId);                                        // NEW
        Task<(int Taken, int Capacity)> GetOfferingOccupancyAsync(int offeringId);                    // NEW

        // Access gating (spec §14) — the seam Quiz / Materials / Sessions will call
        Task<bool> HasActiveAccessAsync(int learnerUserId, int offeringId);                           // NEW
        Task<bool> IsActiveInSubjectAsync(int learnerUserId, int subjectId);                          // NEW


    }
}