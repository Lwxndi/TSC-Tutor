using Tutor_Manager.Models;
using Tutor_Manager.ViewModels;
using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.Services
{
    public interface ISessionService
    {
        // Listing / admin
        Task<List<SessionListItemViewModel>> GetSessionsForOfferingAsync(int offeringId);
        Task<CreateSessionViewModel?> GetCreateFormDataAsync(int offeringId);
        Task<(bool Success, string? Error, int? SessionId)> CreateSessionAsync(CreateSessionViewModel model);
        Task<SessionIndexViewModel> GetSessionIndexAsync(SessionFilterViewModel filter);
        Task<List<OfferingPickerItem>> GetOfferingPickerListAsync();
        Task<SessionDetailViewModel?> GetDetailsAsync(int sessionId);

        // Cancel / reschedule
        Task<(bool Success, string? Error)> CancelSessionAsync(int sessionId, string reason, int? cancelledByUserId = null);
        Task<(bool Success, string? Error)> CancelSessionForTutorAsync(int sessionId, int tutorUserId, string reason);
        Task<RescheduleSessionViewModel?> GetRescheduleFormDataAsync(int sessionId);
        Task<(bool Success, string? Error)> RescheduleSessionAsync(RescheduleSessionViewModel model);

        // Generation
        Task<List<SubjectDropdownItem>> GetActiveSubjectsAsync();
        Task<List<MatchingOfferingItem>> GetMatchingOfferingsAsync(int subjectId, Grade grade);
        Task<GenerationPreviewViewModel> PreviewGenerationAsync(GenerateSessionsRequestViewModel request);
        Task<(int Created, int Skipped)> ConfirmGenerationAsync(List<SessionCandidateViewModel> candidates);
        Task<GenerationPreviewViewModel> GetSystemTimetablePreviewAsync(int? subjectId, Grade? grade, DateTime startDate, DateTime endDate);
        Task<(int Created, int Skipped)> GenerateSessionsForOfferingAsync(int offeringId);
        Task<(int Removed, int Created, int Skipped)> RegenerateFutureSessionsForOfferingAsync(int offeringId);

        // Tutor session flow
        Task<List<SessionListItemViewModel>> GetMySessionsTodayAsync(int tutorUserId);
        Task<TutorSessionBoardViewModel> GetTutorSessionBoardAsync(int tutorUserId);
        Task<(bool Success, string? Error)> StartSessionAsync(int sessionId, int tutorUserId);
        Task<(bool Success, string? Error)> CompleteSessionAsync(int sessionId, int tutorUserId, CompleteSessionViewModel notes);
        Task<SessionListItemViewModel?> GetSessionForTutorAsync(int sessionId, int tutorUserId);

        // Substitutes
        Task<(bool Success, string? Error)> AssignOverrideTutorAsync(int sessionId, int tutorUserId);
        Task<(bool Success, string? Error)> RemoveOverrideTutorAsync(int sessionId);
        Task<List<TutorDropdownItem>> GetAvailableSubstitutesAsync(int sessionId);

        // Timetables
        Task<List<SessionListItemViewModel>> GetTutorTimetableAsync(int tutorUserId, DateTime startDate, DateTime endDate);
        Task<List<SessionListItemViewModel>> GetLearnerTimetableAsync(int learnerUserId, DateTime startDate, DateTime endDate);
        Task<List<LearnerTimetableViewModel>> GetGuardianTimetableAsync(int guardianUserId, DateTime startDate, DateTime endDate);
    }
}