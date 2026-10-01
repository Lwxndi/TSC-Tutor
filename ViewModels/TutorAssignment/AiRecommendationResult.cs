namespace Tutor_Manager.ViewModels.TutorAssignment
{
    public class RecommendedSubjectGrade
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public int Grade { get; set; }
    }

    public class AiRecommendationResult
    {
        public int TutorUserId { get; set; }
        public List<RecommendedSubjectGrade> Recommendations { get; set; } = new();
        public string Reasoning { get; set; } = null!;
        public DateTime DateGenerated { get; set; }
    }
}