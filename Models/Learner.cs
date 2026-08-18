using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class Learner
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Enum instead of a raw number -> renders as a dropdown, and values
        // outside 10-12 simply can't be entered - the enum IS the constraint.
        public Grade? GradeLevel { get; set; }

        [StringLength(100)]
        public string? SchoolName { get; set; }

        public ICollection<LearnerGuardian> Guardians { get; set; } = new List<LearnerGuardian>();

        // Subjects this learner is being tutored in (picked as checkboxes at registration).
        public ICollection<LearnerSubject> Subjects { get; set; } = new List<LearnerSubject>();
    }
}