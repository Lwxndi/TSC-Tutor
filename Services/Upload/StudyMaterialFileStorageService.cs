// Services/Upload/StudyMaterialFileStorageService.cs
using Tutor_Manager.Services.Upload;
using Tutor_Manager.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Tutor_Manager.Services.Upload
{
    public class StudyMaterialFileStorageService : IStudyMaterialFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private const string RootFolder = "StudyMaterials";

        public StudyMaterialFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string referenceNumber)
        {
            var folder = Path.Combine(_env.ContentRootPath, "App_Data", RootFolder, referenceNumber);
            Directory.CreateDirectory(folder);

            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folder, storedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(referenceNumber, storedFileName);
        }

        public string GetFullPath(string storageKey)
        {
            return Path.Combine(_env.ContentRootPath, "App_Data", RootFolder, storageKey);
        }

        // Services/Upload/StudyMaterialFileStorageService.cs — add the implementation
        public void DeleteFile(string storageKey)
        {
            var fullPath = GetFullPath(storageKey);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}