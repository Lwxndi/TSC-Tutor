using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    // A snapshot of what the AI suggested at one point in time — immutable once created.
    public class AiRecommendation
    {
        public int AiRecommendationId { get; set; }

        [ForeignKey("Tutor")]
        public int TutorUserId { get; set; }
        public Tutor Tutor { get; set; } = null!;

        // JSON array of { "SubjectId": 1, "SubjectName": "Mathematics", "Grade": 10 } objects
        public string RecommendationJson { get; set; } = null!;

        public string Reasoning { get; set; } = null!;

        public DateTime DateGenerated { get; set; } = DateTime.Now;
    }
}