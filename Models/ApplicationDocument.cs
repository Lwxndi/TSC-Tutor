using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class ApplicationDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("TutorApplication")]
        public int ApplicationId { get; set; }
        public TutorApplication TutorApplication { get; set; } = null!;

        [Required]
        public ApplicationDocumentType DocumentType { get; set; }

        [Required]
        [StringLength(255)]
        public required string OriginalFileName { get; set; }

        // Deliberately not "FilePath" - never a public/servable path.
        // Retrieved only via an [Authorize(Roles = "Admin")] controller action.
        [Required]
        [StringLength(500)]
        public required string StorageKey { get; set; }

        [Required]
        [StringLength(100)]
        public required string ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}