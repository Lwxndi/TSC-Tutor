
namespace Tutor_Manager.ViewModels
{
    public class AdminDashboardViewModel
    {
        public required string FirstName { get; set; }
        public int PendingApplicationsCount { get; set; }
        public int ChangesRequiredCount { get; set; }
        public int ApprovedTutorsCount { get; set; }
        public int TotalAdmins { get; set; }

        // True only when this admin also holds the Tutor role (e.g. Michael) —
        // lets the view optionally show a link into their own Tutor dashboard too.
        public bool IsAlsoTutor { get; set; }
    }
}