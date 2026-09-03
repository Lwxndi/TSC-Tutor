using Tutor_Manager.ViewModels;
using Tutor_Manager.ViewModels.TutorApplication;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationReviewViewModel
    {
        public int ApplicationId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;

        public TutorApplicationPersonalInfoViewModel PersonalInfo { get; set; } = new();
        public TutorApplicationAcademicInfoViewModel AcademicInfo { get; set; } = new();

        // Read-only projections, not the full posting VMs
        public List<TutorApplicationSubjectRowViewModel> SelectedSubjects { get; set; } = new();
        public List<TutorApplicationExperienceEntryViewModel> Experience { get; set; } = new();
        public List<string> SelectedStrengths { get; set; } = new();
        public string? StrengthsNote { get; set; }
        public List<string> UploadedDocumentNames { get; set; } = new();

        public bool ConsentGiven { get; set; }
    }
}