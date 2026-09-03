using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationSubjectsViewModel : IValidatableObject
    {
        [Required]
        public int ApplicationId { get; set; }

        // Built from ALL Subjects in the controller (GET), so the view can
        // render one row per subject in the table regardless of selection.
        public List<TutorApplicationSubjectRowViewModel> Subjects { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var selected = Subjects.Where(s => s.Selected).ToList();

            if (!selected.Any())
            {
                yield return new ValidationResult(
                    "Select at least one subject.",
                    new[] { nameof(Subjects) });
            }

            foreach (var subject in selected)
            {
                if (subject.GradeLevels == null || !subject.GradeLevels.Any())
                {
                    yield return new ValidationResult(
                        $"Select at least one grade level for {subject.SubjectName}.",
                        new[] { nameof(Subjects) });
                }
            }
        }
    }
}