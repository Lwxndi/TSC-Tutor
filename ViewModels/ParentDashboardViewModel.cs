using System.Collections.Generic;   
namespace Tutor_Manager.ViewModels
{
    public class ParentDashboardViewModel
    {
        
            public required string FirstName { get; set; }
            public required List<LinkedLearnerSummary> Learners { get; set; } = new();

            // Placeholder until session scheduling is built
            public List<string> UpcomingSessions { get; set; } = new();
                
    }

    public class LinkedLearnerSummary
    {
        public required string FullName { get; set; }
        public required string TscNumber { get; set; }
        public required string GradeLevel { get; set; }
    }
}
