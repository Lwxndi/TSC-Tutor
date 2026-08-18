using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Tutor_Manager.Models
{
    public class UserRole
    {
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!; 

        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!; 
    }
}
