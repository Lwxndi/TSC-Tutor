// Services/Upload/IAssessmentFileStorageService.cs
namespace Tutor_Manager.Services.Upload
{
    public interface IAssessmentFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string referenceNumber);
        string GetFullPath(string storageKey);
        void DeleteFile(string storageKey);
    }
}