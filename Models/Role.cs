using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Tutor_Manager.Models
{
    public class Role
    {
        [Key]

        public int RoleId { get; set; }

        [Required]
        [StringLength(20)]
        public required string RoleName { get; set; } // "Tutor" | "Learner" | "Parent" | "Admin"

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
