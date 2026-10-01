using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizQuestionEditViewModel
    {
        public int Id { get; set; } // 0 = new question

        [Required]
        public int QuizId { get; set; }

        [Required]
        public QuizQuestionType QuestionType { get; set; }

        [Required]
        public string QuestionText { get; set; } = null!;

        [Required]
        [Range(1, 100)]
        public int Marks { get; set; }

        public string? MarkingGuidance { get; set; }

        public bool? CorrectBoolAnswer { get; set; }
        public bool HasImage { get; set; }
        public bool RemoveImage { get; set; }
        public IFormFile? ImageFile { get; set; }

        public List<QuizQuestionOptionEditViewModel> Options { get; set; } = new();
    }

    public class QuizQuestionOptionEditViewModel
    {
        public int Id { get; set; } // 0 = new option

        [Required]
        public string OptionText { get; set; } = null!;

        public bool IsCorrect { get; set; }
    }
}