using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels
{
    // Used for the Subject list page — one row per subject, grades shown as badges
    public class SubjectListItemViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public bool IsActive { get; set; }
        public List<Grade> Grades { get; set; } = new();
    }
}