using System.ComponentModel.DataAnnotations;
namespace Tutor_Manager.ViewModels
{
    public class ReplaceTutorViewModel
    {
        public int SessionId { get; set; }

        [Required]
        public int NewTutorUserId { get; set; }

        public List<TutorDropdownItem> AvailableTutors { get; set; } = new();
    }
}