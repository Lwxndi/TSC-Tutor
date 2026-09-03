// ViewModels/TutorDashboardViewModel.cs
using System.Collections.Generic;

namespace Tutor_Manager.ViewModels
{
    public class TutorDashboardViewModel
    {
        public required string FirstName { get; set; }
        public string? TutorNumber { get; set; }
        public required string VettingStatus { get; set; }
        public required string AccountStatus { get; set; }
        public List<string> Subjects { get; set; } = new();

        // Placeholder until session scheduling is built
        public List<string> UpcomingSessions { get; set; } = new();
    }
}