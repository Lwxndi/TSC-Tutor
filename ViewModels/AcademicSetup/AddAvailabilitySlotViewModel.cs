using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class AddAvailabilitySlotViewModel
    {
        public int TutorUserId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }
}