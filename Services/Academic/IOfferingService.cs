//using Tutor_Manager.Models;
//using Tutor_Manager.ViewModels;

//namespace Tutor_Manager.Services.Academic
//{
//    public interface IOfferingService
//    {
//        Task<List<OfferingListItemViewModel>> GetAllOfferingsAsync();
//        Task<OfferingEditViewModel> GetCreateFormDataAsync();
//        Task<OfferingEditViewModel?> GetForEditAsync(int offeringId);

//        Task<(bool Success, string? Error, int? OfferingId)> CreateOfferingAsync(OfferingEditViewModel model);
//        Task<(bool Success, string? Error)> UpdateOfferingAsync(OfferingEditViewModel model);

//        Task<string> SetOfferingActiveStatusAsync(int offeringId, bool isActive);
//        Task<string?> GetTutorEmailAsync(int tutorUserId);

//        Task<List<TutorDropdownItem>> GetEligibleTutorsAsync(int subjectId, Grade? grade);

//        Task<List<OfferingListItemViewModel>> GetAllOfferingsAsync(OfferingFilterViewModel? filter = null);
//        Task<List<TutorDropdownItem>> GetAllTutorsForFilterAsync();
//    }
//}
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Services.Academic
{
    public interface IOfferingService
    {
        Task<List<OfferingListItemViewModel>> GetAllOfferingsAsync(OfferingFilterViewModel? filter = null);
        Task<OfferingEditViewModel> GetCreateFormDataAsync();
        Task<OfferingEditViewModel?> GetForEditAsync(int offeringId);

        Task<(bool Success, string? Error, int? OfferingId)> CreateOfferingAsync(OfferingEditViewModel model);
        Task<(bool Success, string? Error)> UpdateOfferingAsync(OfferingEditViewModel model);

        Task<string> SetOfferingActiveStatusAsync(int offeringId, bool isActive);
        Task<string?> GetTutorEmailAsync(int tutorUserId);

        Task<List<TutorDropdownItem>> GetEligibleTutorsAsync(int subjectId, Grade? grade);
        Task<List<TutorDropdownItem>> GetAllTutorsForFilterAsync();
        Task<List<SubjectDropdownItem>> GetSubjectDropdownAsync();

        Task<List<OfferingListItemViewModel>> GetOfferingsForSubjectGradeAsync(int subjectId, Grade grade);
        Task<List<OfferingListItemViewModel>> GetOfferingsForSubjectAsync(int subjectId);
        Task<List<string>> GetValidStartTimesAsync(DeliveryMethod deliveryMethod, OfferingType type, List<DayOfWeek> days, int durationMinutes, int stepMinutes = 15);

        // Checks a proposed offering (create or edit) against the tutor's
        // existing active offerings and TscSettings caps — sessions/day,
        // subjects/day, hours/day, min break between sessions, subjects/week,
        // and weekly hours. excludeOfferingId lets an edit ignore its own
        // prior state. Backs both save-time enforcement and the live
        // pre-submit check.
        Task<(bool IsValid, string? Error)> ValidateTutorLoadAsync(
            int tutorUserId, int subjectId, OfferingType type, DeliveryMethod deliveryMethod,
            List<DayOfWeek> days, Dictionary<DayOfWeek, TimeSpan?> dayOverrides,
            TimeSpan? weekdayStartTime, TimeSpan? weekendStartTime, TimeSpan legacyStartTime,
            int durationMinutes, int? excludeOfferingId);
    }
}