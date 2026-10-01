using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class AssessmentAttemptAnswer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AssessmentAttempt")]
        public int AssessmentAttemptId { get; set; }
        public AssessmentAttempt AssessmentAttempt { get; set; } = null!;

        [ForeignKey("AssessmentQuestion")]
        public int AssessmentQuestionId { get; set; }
        public AssessmentQuestion AssessmentQuestion { get; set; } = null!;

        // Every AssessmentQuestion is free-text — no MC/TrueFalse fast path exists here.
        public string? TextAnswer { get; set; }

        public int? MarksAwarded { get; set; }

        public string? AiFeedback { get; set; }
    }
}