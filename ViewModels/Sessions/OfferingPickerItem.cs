using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels
{
    public class OfferingPickerItem
    {
        public int OfferingId { get; set; }
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public List<DayOfWeek> TeachingDays { get; set; } = new();
    }
}