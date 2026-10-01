using Tutor_Manager.Models;

namespace Tutor_Manager.ViewModels
{
    public class GradeOptionViewModel
    {
        public Grade Grade { get; set; }
        public string DisplayName { get; set; } = null!; // "Grade 10"
        public bool IsAssigned { get; set; } // for the subject-grade matrix
    }
}