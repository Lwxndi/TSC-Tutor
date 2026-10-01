using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Services
{
    public interface ITutorManagementService
    {
        Task<List<TutorListItemViewModel>> GetAllTutorsAsync();

        Task<TutorSubjectAssignmentViewModel?> GetSubjectAssignmentsAsync(int tutorUserId);
        Task<(bool Success, string? Error)> ToggleSubjectAssignmentAsync(TutorSubjectToggleViewModel model);

        Task<TutorAvailabilityViewModel?> GetAvailabilityAsync(int tutorUserId);
        Task AddAvailabilitySlotAsync(AddAvailabilitySlotViewModel model);
        Task RemoveAvailabilitySlotAsync(int tutorAvailabilityId);

        Task<TutorUnavailabilityViewModel?> GetUnavailabilityAsync(int tutorUserId);
        Task AddUnavailabilityAsync(AddUnavailabilityViewModel model);
        Task RemoveUnavailabilityAsync(int tutorUnavailabilityId);

        Task<string> SetTutorActiveStatusAsync(int tutorUserId, bool isActive);
    }
}