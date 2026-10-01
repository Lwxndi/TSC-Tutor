using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels
{
    public class SubjectViewModel
    {
        public int SubjectId { get; set; }

        [Required]
        [StringLength(50)]
        public string SubjectName { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public List<Grade> AssignedGrades { get; set; } = new();
    }
}