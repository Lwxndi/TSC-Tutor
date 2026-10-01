using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    public class OfferingListItemViewModel
    {
        public int OfferingId { get; set; }
        public string SubjectName { get; set; } = null!;
        public Grade Grade { get; set; }
        public string TutorName { get; set; } = null!;
        public OfferingType Type { get; set; }
        public int Capacity { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public List<DayOfWeek> TeachingDays { get; set; } = new();
    }
}