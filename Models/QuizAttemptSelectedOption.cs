using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class QuizAttemptSelectedOption
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("QuizAttemptAnswer")]
        public int QuizAttemptAnswerId { get; set; }
        public QuizAttemptAnswer QuizAttemptAnswer { get; set; } = null!;

        [ForeignKey("QuizQuestionOption")]
        public int QuizQuestionOptionId { get; set; }
        public QuizQuestionOption QuizQuestionOption { get; set; } = null!;
    }
}