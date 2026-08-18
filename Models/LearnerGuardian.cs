using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class LearnerGuardian
    {
        // Composite key (LearnerUserId, ParentUserId) - annotations can't express a
        // composite key, so this still needs, in OnModelCreating:
        //   modelBuilder.Entity<LearnerGuardian>()
        //       .HasKey(lg => new { lg.LearnerUserId, lg.ParentUserId });

        [ForeignKey("Learner")]
        public int LearnerUserId { get; set; }
        public Learner Learner { get; set; }=null!;

        [ForeignKey("Parent")]
        public int ParentUserId { get; set; }
        public Parent Parent { get; set; } = null!;

        [StringLength(50)]
        public string? RelationshipToLearner { get; set; } // "Mother", "Father", "Guardian"...

        public bool IsPrimaryContact { get; set; }


    }
}
