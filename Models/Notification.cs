// Models/Notification.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        [Required]
        [StringLength(500)]
        public required string Message { get; set; }

        // Optional - where clicking the notification should take the user
        // e.g. "/Sessions/Details/12"
        public string? Link { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}