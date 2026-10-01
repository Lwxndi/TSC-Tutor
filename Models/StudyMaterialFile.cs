using System;
using Tutor_Manager.Models.Enums;
namespace Tutor_Manager.Models
{
    public class StudyMaterialFile
    {
        public int Id { get; set; }
        public int StudyMaterialId { get; set; }
        public StudyMaterial StudyMaterial { get; set; }
        public string FileName { get; set; }
        public string StoragePath { get; set; }
        public MaterialFileType FileType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
