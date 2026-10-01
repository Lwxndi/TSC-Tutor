using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    // Posted from the Subject edit form — checkboxes for Grade10/11/12
    public class SubjectEditViewModel
    {
        public int SubjectId { get; set; }

        [Required]
        [StringLength(50)]
        public string SubjectName { get; set; } = null!;

        public bool IsActive { get; set; }

        public bool Grade10 { get; set; }
        public bool Grade11 { get; set; }
        public bool Grade12 { get; set; }
    }
}