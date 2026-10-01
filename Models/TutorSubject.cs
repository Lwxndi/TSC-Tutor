using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class TutorSubject
    {
        [ForeignKey("Tutor")]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; } = null!;

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
        public int? AssignedByUserId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public Grade GradeLevel { get; set; }
    }
}