// ViewModels/AssessmentViewmodels/AssessmentReviewViewModel.cs
using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels.AssessmentViewmodels
{
    public class AssessmentReviewViewModel
    {
        public int AssessmentId { get; set; }
        public string AssessmentTitle { get; set; } = null!;
        public AssessmentGenerationStatus Status { get; set; }
        public int? DeclaredTotalMarks { get; set; }
        public int ComputedTotalMarks { get; set; } // Sum of question marks — see Decision 4
        public List<AssessmentQuestionSummaryViewModel> Questions { get; set; } = new();
    }

    public class AssessmentQuestionSummaryViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = null!;
        public int Marks { get; set; }
        public int DisplayOrder { get; set; }
    }
}