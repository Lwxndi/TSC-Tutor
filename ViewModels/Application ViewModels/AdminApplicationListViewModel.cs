using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class AdminApplicationListViewModel
    {
        public List<AdminApplicationListItemViewModel> Applications { get; set; } = new();
        public ApplicationStatus? StatusFilter { get; set; }
    }
}