using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels
{
    public class TutorSubjectAssignmentViewModel
    {
        public int TutorUserId { get; set; }
        public string TutorName { get; set; } = null!;

        public List<SubjectAssignmentRow> Subjects { get; set; } = new();
    }

    public class SubjectAssignmentRow
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;

        // Which grades TSC actually offers this subject for (from SubjectGrade)
        public List<Grade> OfferedGrades { get; set; } = new();

        // Which of those grades this tutor is currently assigned to teach
        public List<Grade> AssignedGrades { get; set; } = new();
    }
}