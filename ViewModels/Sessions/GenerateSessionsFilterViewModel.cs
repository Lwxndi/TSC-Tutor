using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels.Sessions
{
    // Step 1: filter form + matching offerings shown on Generate.cshtml
    public class GenerateSessionsFilterViewModel
    {
        public int? SubjectId { get; set; }
        public Grade? Grade { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

        public List<SubjectDropdownItem> AvailableSubjects { get; set; } = new();
        public List<MatchingOfferingItem> MatchingOfferings { get; set; } = new();
    }

    public class MatchingOfferingItem
    {
        public int OfferingId { get; set; }
        public string TutorName { get; set; } = null!;
        public List<DayOfWeek> TeachingDays { get; set; } = new();
        public DeliveryMethod DeliveryMethod { get; set; }
    }

    // Posted from Generate.cshtml to PreviewGeneration
    public class GenerateSessionsRequestViewModel
    {
        public int SubjectId { get; set; }
        public Grade Grade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> SelectedOfferingIds { get; set; } = new();
        public List<DayTimeSlot> DayTimePatterns { get; set; } = new();
    }

    public class DayTimeSlot
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    // One generated candidate session, valid or not, shown on Preview.cshtml
    public class SessionCandidateViewModel
    {
        public int OfferingId { get; set; }
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsValid { get; set; }
        public string? Error { get; set; }
    }

    public class GenerationPreviewViewModel
    {
        public List<SessionCandidateViewModel> ValidCandidates { get; set; } = new();
        public List<SessionCandidateViewModel> InvalidCandidates { get; set; } = new();
    }
}