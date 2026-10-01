namespace Tutor_Manager.ViewModels.TutorAssignment
{
    public enum TutorAssignmentStatus
    {
        Unassigned,
        Assigned,
        OnLeave,
        Inactive
    }

    public class TutorAssignmentListItem
    {
        public int TutorUserId { get; set; }
        public string FullName { get; set; } = null!;
        public TutorAssignmentStatus Status { get; set; }
        public List<string> CurrentAssignments { get; set; } = new(); // e.g. "Mathematics (Grade 10)"
        public bool HasRecommendation { get; set; }
    }

    public class TutorAssignmentIndexViewModel
    {
        public List<TutorAssignmentListItem> Unassigned { get; set; } = new();
        public List<TutorAssignmentListItem> Assigned { get; set; } = new();
        public List<TutorAssignmentListItem> OnLeave { get; set; } = new();
        public List<TutorAssignmentListItem> Inactive { get; set; } = new();
    }

    public class ManageTutorViewModel
    {
        public int TutorUserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Qualification { get; set; }
        public string? Bio { get; set; }
        public string? TutorNumber { get; set; }
        public bool IsActive { get; set; }

        public List<string> CurrentAssignments { get; set; } = new();
        public AiRecommendationResult? LatestRecommendation { get; set; }
    }
}