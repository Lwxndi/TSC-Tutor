using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels.AssessmentViewmodels
{
    public class AssessmentEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly DueDate { get; set; }

        [Range(1, 1000)]
        public int? DeclaredTotalMarks { get; set; }

        public string ExistingQuestionPaperFileName { get; set; } = null!;
        public string ExistingMarkingGuidelineFileName { get; set; } = null!;

        public IFormFile? NewQuestionPaperFile { get; set; }
        public IFormFile? NewMarkingGuidelineFile { get; set; }

        // --- Added in Step 10 ---
        [Required]
        public int TimezoneOffsetMinutes { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly OpenDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeOnly OpenTime { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly CloseDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public TimeOnly CloseTime { get; set; }

        public DateTime OpenAt => OpenDate.ToDateTime(OpenTime).AddMinutes(TimezoneOffsetMinutes);
        public DateTime CloseAt => CloseDate.ToDateTime(CloseTime).AddMinutes(TimezoneOffsetMinutes);

        [Required]
        [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10.")]
        public int MaxAttempts { get; set; }

        [Required]
        public QuizzTypes AudienceType { get; set; }

        public int? OfferingId { get; set; }

        public List<int> SelectedLearnerUserIds { get; set; } = new();
        public List<int> ExistingLearnerUserIds { get; set; } = new();

        public List<SelectListItem> AvailableSubjects { get; set; } = new();
    }
}