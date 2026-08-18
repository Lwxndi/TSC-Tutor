using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace Tutor_Manager.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
 
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public required string FirstName { get; set; }
 
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public required string LastName { get; set; }
 
        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public required string Email { get; set; }
 
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public required string PhoneNumber { get; set; }
 
        [StringLength(15)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? AltPhoneNumber { get; set; }
 
        // Stores the HASHED password only. Strength rules (uppercase, digit, special
        // char, min length) belong on a separate RegisterViewModel.Password property
        // that validates the RAW input before it gets hashed - a hash will never
        // satisfy those rules, so they can't live here.
        [Required]
        public required string PasswordHash { get; set; }
 
        // Applies to every user type, unlike Grade (Learner-only) or Subjects (Tutor/Learner-only).
        public Gender? Gender { get; set; }
 
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
 
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
 
        public Tutor? Tutor { get; set; }
        public Parent? Parent { get; set; }
        public Learner? Learner { get; set; }
        public Administrator? Administrator { get; set; }
    }
}
