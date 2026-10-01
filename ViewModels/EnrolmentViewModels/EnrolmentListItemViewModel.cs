using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    public class EnrolmentListItemViewModel
    {
        public int EnrolmentId { get; set; }
        public string LearnerName { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EnrollmentStatus Status { get; set; }
        public decimal Price { get; set; }
    }

    public class EnrollLearnerViewModel
    {
        public int LearnerUserId { get; set; }
        public int OfferingId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;

        public List<LearnerDropdownItem> AvailableLearners { get; set; } = new();
        public List<OfferingDropdownItem> AvailableOfferings { get; set; } = new();
    }

    public class LearnerDropdownItem
    {
        public int LearnerUserId { get; set; }
        public string FullName { get; set; } = null!;
        public Grade? GradeLevel { get; set; }
    }

    public class OfferingDropdownItem
    {
        public int OfferingId { get; set; }
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public int Capacity { get; set; }
        public int CurrentEnrolments { get; set; }
        public bool IsFull => CurrentEnrolments >= Capacity;
    }
}