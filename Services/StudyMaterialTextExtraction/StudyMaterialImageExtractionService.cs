using DocumentFormat.OpenXml.Packaging;
using System.Security.Cryptography;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Upload;
using UglyToad.PdfPig;

namespace Tutor_Manager.Services.StudyMaterialTextExtraction
{
    public record ExtractedImage(int Ref, string MimeType, byte[] Bytes);

    public interface IStudyMaterialImageExtractionService
    {
        List<ExtractedImage> Extract(StudyMaterial material);
    }

    public class StudyMaterialImageExtractionService : IStudyMaterialImageExtractionService
    {
        private const int MaxImages = 20, MinBytes = 4_000, MaxBytes = 2_000_000;
        private static readonly HashSet<string> Supported = new() { "image/png", "image/jpeg", "image/webp" };

        private readonly IStudyMaterialFileStorageService _fileStorage;
        private readonly ILogger<StudyMaterialImageExtractionService> _logger;

        public StudyMaterialImageExtractionService(IStudyMaterialFileStorageService fileStorage,
            ILogger<StudyMaterialImageExtractionService> logger)
        { _fileStorage = fileStorage; _logger = logger; }

        public List<ExtractedImage> Extract(StudyMaterial material)
        {
            var found = new List<(string Mime, byte[] Bytes)>();
            var seen = new HashSet<string>();

            void Add(string mime, byte[] bytes)
            {
                if (found.Count >= MaxImages || !Supported.Contains(mime)) return;
                if (bytes.Length < MinBytes || bytes.Length > MaxBytes) return; // skip icons and huge images
                if (seen.Add(Convert.ToHexString(SHA256.HashData(bytes)))) found.Add((mime, bytes));
            }

            foreach (var file in material.Files)
            {
                var path = _fileStorage.GetFullPath(file.StoragePath);
                if (!File.Exists(path)) continue;
                try
                {
                    switch (file.FileType)
                    {
                        case MaterialFileType.Word: FromWord(path, Add); break;
                        case MaterialFileType.PowerPoint: FromPowerPoint(path, Add); break;
                        case MaterialFileType.Pdf: FromPdf(path, Add); break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Image extraction failed for {File}", file.FileName);
                }
            }

            return found.Select((x, i) => new ExtractedImage(i + 1, x.Mime, x.Bytes)).ToList();
        }

        private static byte[] Read(ImagePart part)
        {
            using var s = part.GetStream();
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        private static void FromWord(string path, Action<string, byte[]> add)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var main = doc.MainDocumentPart;
            if (main?.Document == null) return;
            foreach (var blip in main.Document.Descendants<DocumentFormat.OpenXml.Drawing.Blip>())
            {
                var id = blip.Embed?.Value;
                if (id != null && main.GetPartById(id) is ImagePart ip) add(ip.ContentType, Read(ip));
            }
        }

        private static void FromPowerPoint(string path, Action<string, byte[]> add)
        {
            using var doc = PresentationDocument.Open(path, false);
            var pres = doc.PresentationPart;
            if (pres?.Presentation.SlideIdList == null) return;
            foreach (var slideId in pres.Presentation.SlideIdList.Elements<DocumentFormat.OpenXml.Presentation.SlideId>())
            {
                if (slideId.RelationshipId?.Value is not string rel) continue;
                var slidePart = (SlidePart)pres.GetPartById(rel);
                foreach (var blip in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Blip>())
                {
                    var id = blip.Embed?.Value;
                    if (id != null && slidePart.GetPartById(id) is ImagePart ip) add(ip.ContentType, Read(ip));
                }
            }
        }

        private static void FromPdf(string path, Action<string, byte[]> add)
        {
            using var pdf = PdfDocument.Open(path);
            foreach (var page in pdf.GetPages())
                foreach (var img in page.GetImages())
                    if (img.TryGetPng(out var png) && png != null) add("image/png", png);
        }
    }
}