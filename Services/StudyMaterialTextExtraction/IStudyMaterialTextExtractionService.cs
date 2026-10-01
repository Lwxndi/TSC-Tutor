// Services/StudyMaterialTextExtraction/IStudyMaterialTextExtractionService.cs
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Services.StudyMaterialTextExtraction
{
    public interface IStudyMaterialTextExtractionService
    {
        Task<string> ExtractTextAsync(StudyMaterial material);

        // New: works off a plain file path — reusable by any feature with its own storage folder
        string ExtractFileText(string fullPath, MaterialFileType fileType);
    }
}