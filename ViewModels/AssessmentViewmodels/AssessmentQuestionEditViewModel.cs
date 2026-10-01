// ViewModels/AssessmentViewmodels/AssessmentQuestionEditViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.AssessmentViewmodels
{
    public class AssessmentQuestionEditViewModel
    {
        public int Id { get; set; } // 0 = new question

        [Required]
        public int AssessmentId { get; set; }

        [Required]
        public string QuestionText { get; set; } = null!;

        [Required]
        [Range(1, 100)]
        public int Marks { get; set; }

        [Required]
        public string MarkingGuidance { get; set; } = null!;
    }
}