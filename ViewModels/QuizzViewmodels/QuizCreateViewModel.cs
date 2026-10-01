// ViewModels/QuizzViewmodels/QuizCreateViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tutor_Manager.Helpers;
using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizCreateViewModel
    {
        [Required]
        public int StudyMaterialId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly OpenDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeOnly OpenTime { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly CloseDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeOnly CloseTime { get; set; }

        // Computed — the tutor enters SAST wall-clock time; these two lines convert
        // to true UTC before the Service ever sees them, so DateTime.UtcNow comparisons
        // downstream are correct. This is the only change in this file.
        public DateTime OpenAt => SouthAfricaTime.ToUtc(OpenDate.ToDateTime(OpenTime));
        public DateTime CloseAt => SouthAfricaTime.ToUtc(CloseDate.ToDateTime(CloseTime));

        [Required]
        [Range(1, 300, ErrorMessage = "Duration must be between 1 and 300 minutes.")]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10.")]
        public int MaxAttempts { get; set; }

        [Required]
        public QuizzTypes AudienceType { get; set; }

        public int? OfferingId { get; set; }

        [Required]
        public QuizDifficultyLevel DifficultyLevel { get; set; }

        [Range(0, 50)]
        public int MultipleChoiceSingleCount { get; set; }

        [Range(0, 50)]
        public int MultipleChoiceMultipleCount { get; set; }

        [Range(0, 50)]
        public int TrueFalseCount { get; set; }

        [Range(0, 50)]
        public int ShortAnswerCount { get; set; }

        [Range(0, 50)]
        public int EssayCount { get; set; }

        [Range(0, 50)]
        public int NumericEquationCount { get; set; }

        public List<int> SelectedLearnerUserIds { get; set; } = new();

        public List<SelectListItem> AvailableStudyMaterials { get; set; } = new();
        public List<SelectListItem> AvailableOfferings { get; set; } = new();
        public List<SelectListItem> AvailableLearners { get; set; } = new();
    }
}