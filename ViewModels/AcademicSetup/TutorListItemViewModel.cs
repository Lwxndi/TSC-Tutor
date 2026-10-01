namespace Tutor_Manager.ViewModels
{
    public class TutorListItemViewModel
    {
        public int TutorUserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? TutorNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> SubjectGradeSummary { get; set; } = new(); // e.g. "Mathematics (Grade 11, 12)"
    }
}