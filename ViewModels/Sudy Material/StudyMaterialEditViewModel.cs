using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Tutor_Manager.Services;
//using Tutor_Manager.Validation;

namespace Tutor_Manager.ViewModels
{
    public class StudyMaterialEditViewModel
    {
        public int Id { get; set; }

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

        public List<ExistingFileViewModel> ExistingFiles { get; set; } = new();

        // Posted back: Ids of existing files the tutor unchecked (wants removed)
        public List<int> FileIdsToRemove { get; set; } = new();

        [MaxFileCount(10)]
        public List<IFormFile> NewFiles { get; set; } = new();

        public List<SelectListItem> AvailableSubjects { get; set; } = new();
    }

    public class ExistingFileViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
    }
}