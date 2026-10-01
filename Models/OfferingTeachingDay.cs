using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class OfferingTeachingDay
    {
        public int OfferingTeachingDayId { get; set; }

        [ForeignKey("Offering")]
        public int OfferingId { get; set; }
        public Offering Offering { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        // When set, this specific day ignores Offering.WeekdayStartTime /
        // WeekendStartTime entirely and uses this time instead.
        public TimeSpan? StartTimeOverride { get; set; }
    }
}