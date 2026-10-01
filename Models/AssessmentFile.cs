// Models/AssessmentFile.cs
using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Models
{
    public class AssessmentFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; } = null!;

        [Required]
        public string FileName { get; set; } = null!;

        [Required]
        public string StoragePath { get; set; } = null!;

        [Required]
        public MaterialFileType FileType { get; set; } // reused from StudyMaterialFile

        [Required]
        public AssessmentFileRole Role { get; set; }

        public DateTime UploadedAt { get; set; }
    }

    public enum AssessmentFileRole
    {
        QuestionPaper,
        MarkingGuideline
    }
}