using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    // Pure bridge: Between Subject and Grade. This is used to define which subjects are available for each grade level.

    public class SubjectGrade
    {
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; } = null!;

        public Grade Grade { get; set; }
    }
}