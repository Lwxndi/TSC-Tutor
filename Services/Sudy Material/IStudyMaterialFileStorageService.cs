// Services/Upload/IStudyMaterialFileStorageService.cs — add one method
namespace Tutor_Manager.Services.Upload
{
    public interface IStudyMaterialFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string referenceNumber);
        string GetFullPath(string storageKey);
        void DeleteFile(string storageKey);   // new
    }
}