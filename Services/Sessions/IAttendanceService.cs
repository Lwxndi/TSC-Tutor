using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.Services
{
    public interface IAttendanceService
    {
        Task<MarkAttendanceViewModel?> GetForTutorAsync(int sessionId, int tutorUserId);
        Task<MarkAttendanceViewModel?> GetForAdminAsync(int sessionId);
        Task<(bool Success, string? Error)> SaveForTutorAsync(MarkAttendanceViewModel model, int tutorUserId);
        Task<(bool Success, string? Error)> SaveForAdminAsync(MarkAttendanceViewModel model, int adminUserId);
        Task<int> GetUnmarkedCountAsync(int sessionId);
    }
}