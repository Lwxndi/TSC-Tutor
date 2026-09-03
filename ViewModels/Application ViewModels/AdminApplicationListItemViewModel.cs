using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class AdminApplicationListItemViewModel
    {
        public int ApplicationId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public LocationPreference LocationPreference { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime? DateApplied { get; set; }
    }
}