using Tutor_Manager.Models.Enums;
using Tutor_Manager.ViewModels.TutorApplication;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class AdminApplicationDetailViewModel
    {
        public int ApplicationId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public ApplicationStatus Status { get; set; }

        public TutorApplicationPersonalInfoViewModel PersonalInfo { get; set; } = new();
        public TutorApplicationAcademicInfoViewModel AcademicInfo { get; set; } = new();
        public List<TutorApplicationSubjectRowViewModel> Subjects { get; set; } = new();
        public List<TutorApplicationExperienceEntryViewModel> Experience { get; set; } = new();
        public List<string> SelectedStrengths { get; set; } = new();
        public string? StrengthsNote { get; set; }

        // name + a link to the authorised download action, not a public path
        public List<(string DocumentType, string FileName, string DownloadUrl)> Documents { get; set; } = new();

        public bool ConsentGiven { get; set; }
        public DateTime? ConsentDate { get; set; }

        public DateTime? DateApplied { get; set; }
        public DateTime? DateReviewed { get; set; }
        public string? ReviewedByAdminName { get; set; }

        // Populated only once Status == Approved
        public int? CreatedTutorId { get; set; }
        public string? CreatedTutorNumber { get; set; }
    }
}