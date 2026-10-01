using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class AddUnavailabilityViewModel
    {
        public int TutorUserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [StringLength(255)]
        public string? Reason { get; set; }
    }
}