using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Services
{
    public interface IEnrolmentService
    {
        Task<List<EnrolmentListItemViewModel>> GetAllEnrolmentsAsync();
        Task<EnrollLearnerViewModel> GetEnrollFormDataAsync();
        Task<(bool Success, string? Error, int? EnrolmentId)> EnrollLearnerAsync(EnrollLearnerViewModel model);
        Task<(bool Success, string? Error)> CancelEnrolmentAsync(int enrolmentId, string reason);
        Task<List<EnrolmentListItemViewModel>> GetEnrolmentsForLearnerAsync(int learnerUserId);
        Task<List<EnrolmentListItemViewModel>> GetEnrolmentsForOfferingAsync(int offeringId);
    }
}