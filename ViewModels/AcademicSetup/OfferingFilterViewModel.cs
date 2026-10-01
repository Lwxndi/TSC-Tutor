using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    public class OfferingFilterViewModel
    {
        public int? SubjectId { get; set; }
        public Grade? Grade { get; set; }
        public int? TutorUserId { get; set; }
        public OfferingType? Type { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }
        public bool? IsActive { get; set; } // null = all, true = active only, false = inactive only
    }
}