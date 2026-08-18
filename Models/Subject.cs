using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(50)]
        public required string SubjectName { get; set; }

        public ICollection<TutorSubject> Tutors { get; set; } = new List<TutorSubject>();
    }
}
