// Services/StudyMaterialTextExtraction/StudyMaterialTextExtractionService.cs
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Upload;
using UglyToad.PdfPig;

namespace Tutor_Manager.Services.StudyMaterialTextExtraction
{
    public class StudyMaterialTextExtractionService : IStudyMaterialTextExtractionService
    {
        private readonly IStudyMaterialFileStorageService _fileStorage;
        private const int MaxCharsPerMaterial = 60000;

        public StudyMaterialTextExtractionService(IStudyMaterialFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task<string> ExtractTextAsync(StudyMaterial material)
        {
            var sb = new StringBuilder();

            foreach (var file in material.Files)
            {
                var fullPath = _fileStorage.GetFullPath(file.StoragePath);
                if (!File.Exists(fullPath))
                    continue;

                sb.AppendLine($"--- File: {file.FileName} ---");
                sb.AppendLine(ExtractFileText(fullPath, file.FileType));
                sb.AppendLine();
            }

            var combined = sb.ToString();
            if (combined.Length > MaxCharsPerMaterial)
                combined = combined.Substring(0, MaxCharsPerMaterial);

            return await Task.FromResult(combined);
        }

        public string ExtractFileText(string fullPath, MaterialFileType fileType)
        {
            if (!File.Exists(fullPath))
                return string.Empty;

            return fileType switch
            {
                MaterialFileType.Pdf => ExtractPdfText(fullPath),
                MaterialFileType.Word => ExtractWordText(fullPath),
                MaterialFileType.PowerPoint => ExtractPowerPointText(fullPath),
                _ => string.Empty
            };
        }

        private static string ExtractPdfText(string path)
        {
            var sb = new StringBuilder();
            using var pdf = PdfDocument.Open(path);
            foreach (var page in pdf.GetPages())
                sb.AppendLine(page.Text);
            return sb.ToString();
        }

        private static string ExtractWordText(string path)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            return body?.InnerText ?? string.Empty;
        }

        private static string ExtractPowerPointText(string path)
        {
            var sb = new StringBuilder();
            using var doc = PresentationDocument.Open(path, false);
            var presentationPart = doc.PresentationPart;
            if (presentationPart?.Presentation.SlideIdList == null)
                return string.Empty;

            foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<DocumentFormat.OpenXml.Presentation.SlideId>())
            {
                var relId = slideId.RelationshipId?.Value;
                if (relId == null) continue;

                var slidePart = (SlidePart)presentationPart.GetPartById(relId);
                foreach (var t in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                    sb.AppendLine(t.Text);
            }
            return sb.ToString();
        }
    }
}