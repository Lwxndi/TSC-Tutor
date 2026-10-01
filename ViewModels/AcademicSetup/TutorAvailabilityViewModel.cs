using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class TutorAvailabilityViewModel
    {
        public int TutorUserId { get; set; }
        public string TutorName { get; set; } = null!;
        public List<AvailabilitySlotViewModel> Slots { get; set; } = new();
    }

    public class AvailabilitySlotViewModel
    {
        public int TutorAvailabilityId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }
}