namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationSubjectRowViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;

        public bool Selected { get; set; }

        // Checkbox group per subject, e.g. ["10","11"]. Maps to/from the
        // entity's comma-delimited GradeLevels string in the controller.
        public List<string> GradeLevels { get; set; } = new();

        public string? CompetencyNote { get; set; }
        public string? ResultNote { get; set; }
    }
}