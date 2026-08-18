namespace Tutor_Manager.ViewModels
{
    // One checkbox row in the subject picker. The controller populates this list
    // from the Subjects table on GET; on POST, IsSelected comes back per item
    // and the controller filters .Where(s => s.IsSelected) to build LearnerSubject rows.
    public class SubjectSelection
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
