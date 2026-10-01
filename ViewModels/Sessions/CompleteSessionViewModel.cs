using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels.Sessions
{
    public class CompleteSessionViewModel
    {
        public int SessionId { get; set; }
        [StringLength(2000)]
        public string? Notes { get; set; }
        public string? TopicsCovered { get; set; }
        public StudentPerformance? Performance { get; set; }
        public string? HomeworkAssigned { get; set; }
        public string? TutorComments { get; set; }
    }
}