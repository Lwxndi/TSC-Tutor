using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    public class OfferingEditViewModel
    {
        public int OfferingId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public Grade Grade { get; set; }

        [Required]
        public int TutorUserId { get; set; }

        [Required]
        public OfferingType Type { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; } = 1;

        [Required]
        public DeliveryMethod DeliveryMethod { get; set; }

        [Range(15, 480)]
        public int DurationMinutes { get; set; }

        // Legacy fallback — still bound/persisted, but Weekday/WeekendStartTime
        // (below) are what actually drive validation and session generation now.
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        // Default time applied to every selected weekday teaching day that
        // doesn't have its own override in DayOverrides.
        [DataType(DataType.Time)]
        public TimeSpan? WeekdayStartTime { get; set; }

        // Default time applied to every selected weekend teaching day that
        // doesn't have its own override in DayOverrides.
        [DataType(DataType.Time)]
        public TimeSpan? WeekendStartTime { get; set; }

        // Per-day exceptions — e.g. "Wednesday is 15:00 instead of the weekday default."
        // Keyed by DayOfWeek; a missing or null entry means "use the tiered default."
        public Dictionary<DayOfWeek, TimeSpan?> DayOverrides { get; set; } = new();

        [Range(0, 100000)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

        public BillingType BillingType { get; set; } = BillingType.Recurring;
        public List<DayOfWeek> SelectedTeachingDays { get; set; } = new();

        public List<SubjectDropdownItem> AvailableSubjects { get; set; } = new();
        public List<TutorDropdownItem> AvailableTutors { get; set; } = new();
    }

    public class SubjectDropdownItem
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
    }

    public class TutorDropdownItem
    {
        public int TutorUserId { get; set; }
        public string FullName { get; set; } = null!;
    }
}