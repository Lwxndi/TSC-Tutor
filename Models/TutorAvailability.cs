using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class TutorAvailability
    {
        public int TutorAvailabilityId { get; set; }

        [ForeignKey("Tutor")]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}