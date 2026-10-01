using Tutor_Manager.ViewModels.TutorAssignment;

namespace Tutor_Manager.Services
{
    public interface ITutorAssignmentService
    {
        Task<TutorAssignmentIndexViewModel> GetGroupedTutorsAsync();
        Task<ManageTutorViewModel?> GetManageTutorAsync(int tutorUserId);
        Task<(bool Success, string? Error)> AssignAsync(int tutorUserId, int subjectId, Models.Grade grade, int assignedByUserId);
        Task<(bool Success, string? Warning)> CheckRemovalImpactAsync(int tutorUserId, int subjectId, Models.Grade grade);
    }
}