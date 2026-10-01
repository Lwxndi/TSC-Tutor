using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Models
{
    public class Offering
    {
        public int OfferingId { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public Grade Grade { get; set; }

        [ForeignKey("Tutor")]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; } = null!;

        public OfferingType Type { get; set; }

        public int Capacity { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; }

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BillingType BillingType { get; set; } = BillingType.Recurring;


        public TimeSpan StartTime { get; set; }

       
        public TimeSpan? WeekdayStartTime { get; set; }

       
        public TimeSpan? WeekendStartTime { get; set; }

        [NotMapped]
        public TimeSpan EndTime => StartTime.Add(TimeSpan.FromMinutes(DurationMinutes));

        public ICollection<OfferingTeachingDay> TeachingDays { get; set; } = new List<OfferingTeachingDay>();
    }
}