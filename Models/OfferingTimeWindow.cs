using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class OfferingTimeWindow
    {
        public int OfferingTimeWindowId { get; set; }

        public DayType DayType { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public OfferingType OfferingType { get; set; }

        public TimeSpan WindowStart { get; set; }
        public TimeSpan WindowEnd { get; set; }
    }
}