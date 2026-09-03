using System.Collections.Generic;

namespace Tutor_Manager.ViewModels
{
    public class LearnerDashboardViewModel
    {
        public required string FirstName { get; set; }
        public required string TscNumber { get; set; }
        public required string GradeLevel { get; set; }
        public required string SchoolName { get; set; }
        public List<string> Subjects { get; set; } = new();
        public List<GuardianSummary> Guardians { get; set; } = new();

        // Placeholder until session scheduling is built
        public List<string> UpcomingSessions { get; set; } = new();
    }

    public class GuardianSummary
    {
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public string? Relationship { get; set; }
    }
}