namespace Tutor_Manager.Services.Upload
{
   public interface IApplicationFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string referenceNumber);
        string GetFullPath(string storageKey);
    }
}
