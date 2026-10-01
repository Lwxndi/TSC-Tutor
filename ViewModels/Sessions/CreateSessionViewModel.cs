using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    public class CreateSessionViewModel
    {
        public int OfferingId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        // Display-only — never posted back from the form, so excluded from validation
        [ValidateNever]
        public string SubjectName { get; set; } = null!;

        [ValidateNever]
        public Grade Grade { get; set; }

        [ValidateNever]
        public string TutorName { get; set; } = null!;

        [ValidateNever]
        public List<DayOfWeek> OfferingTeachingDays { get; set; } = new();

        [ValidateNever]
        public DeliveryMethod DeliveryMethod { get; set; }
    }
}