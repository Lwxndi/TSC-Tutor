using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class TutorSubject
    {
        [ForeignKey("Tutor")]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; }= null!;

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        [Range(10, 12, ErrorMessage = "Grade level must be between 10 and 12.")]
        public byte GradeLevel { get; set; }
    }
}
