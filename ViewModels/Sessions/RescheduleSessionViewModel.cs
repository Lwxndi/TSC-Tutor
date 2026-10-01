using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class RescheduleSessionViewModel
    {
        public int SessionId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NewDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan NewStartTime { get; set; }
        public string? RescheduleReason { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan NewEndTime { get; set; }
    }
}