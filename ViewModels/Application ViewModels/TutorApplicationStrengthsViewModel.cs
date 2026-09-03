using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationStrengthsViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        // Rendered against a fixed list in the view (problem solving, exam
        // prep, etc.) - no lookup table, per Stage 7's "admin reading
        // material, not a scored input" note.
        public List<string> SelectedStrengths { get; set; } = new();

        [StringLength(2000)]
        public string? StrengthsNote { get; set; }
    }
}