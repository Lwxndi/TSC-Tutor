// Models/AccountActivationToken.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tutor_Manager.Models
{
    public class AccountActivationToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Token { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}