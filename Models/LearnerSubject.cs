using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class LearnerSubject
    {
        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
    }
}
