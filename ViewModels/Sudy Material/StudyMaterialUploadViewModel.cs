// ViewModels/StudyMaterialUploadViewModel.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Services;

namespace Tutor_Manager.ViewModels
{
    public class StudyMaterialUploadViewModel
    {
        [Required]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(200)]
        public string TopicChapterLabel { get; set; }

        [Required]
        [MaxFileCount(10)]
        public List<IFormFile> Files { get; set; } = new();

        public List<SelectListItem> AvailableSubjects { get; set; } = new();
    }
}