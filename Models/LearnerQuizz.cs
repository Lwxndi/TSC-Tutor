// Models/LearnerQuizz.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class LearnerQuizz
    {
        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; } = null!;
    }
}