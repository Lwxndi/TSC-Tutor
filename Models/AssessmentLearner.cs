using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class AssessmentLearner
    {
        [ForeignKey("Assessment")]
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; } = null!;

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;
    }
}