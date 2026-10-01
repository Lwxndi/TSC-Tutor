using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class CancelSessionViewModel
    {
        public int SessionId { get; set; }

        [Required]
        [StringLength(255)]
        public string Reason { get; set; } = null!;
    }
}