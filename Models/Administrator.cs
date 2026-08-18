using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class Administrator
    {
        // Shared primary key: UserId is both this table's PK and its FK to Users.
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [StringLength(50)]
        public string? Position { get; set; }
    }
}
