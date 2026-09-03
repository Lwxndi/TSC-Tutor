using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationDocumentUploadViewModel : IValidatableObject
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        [Required]
        public int ApplicationId { get; set; }

        // Required slots
        public IFormFile? CV { get; set; }
        public IFormFile? Transcript { get; set; }

        // Recommended / optional slots
        public IFormFile? Certificate { get; set; }
        public IFormFile? TeachingCertificate { get; set; }
        public IFormFile? ReferenceLetter { get; set; }
        public IFormFile? Other { get; set; }

        // Populated by the controller (GET) so the view can show "✓ uploaded"
        // for slots already satisfied on a previous visit, without forcing
        // re-upload. Keyed by DocumentType name, e.g. "CV" -> "my_cv.pdf".
        public Dictionary<string, string> AlreadyUploadedFileNames { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool CvSatisfied = CV != null || AlreadyUploadedFileNames.ContainsKey("CV");
            bool TranscriptSatisfied = Transcript != null || AlreadyUploadedFileNames.ContainsKey("Transcript");

            if (!CvSatisfied)
                yield return new ValidationResult("CV is required.", new[] { nameof(CV) });

            if (!TranscriptSatisfied)
                yield return new ValidationResult("Academic transcript is required.", new[] { nameof(Transcript) });

            foreach (var result in ValidateFile(CV, nameof(CV)))
                yield return result;
            foreach (var result in ValidateFile(Transcript, nameof(Transcript)))
                yield return result;
            foreach (var result in ValidateFile(Certificate, nameof(Certificate)))
                yield return result;
            foreach (var result in ValidateFile(TeachingCertificate, nameof(TeachingCertificate)))
                yield return result;
            foreach (var result in ValidateFile(ReferenceLetter, nameof(ReferenceLetter)))
                yield return result;
            foreach (var result in ValidateFile(Other, nameof(Other)))
                yield return result;
        }

        private static IEnumerable<ValidationResult> ValidateFile(IFormFile? file, string fieldName)
        {
            if (file == null) yield break;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                yield return new ValidationResult(
                    $"{fieldName}: only PDF, JPG, or PNG files are allowed.",
                    new[] { fieldName });
            }

            if (file.Length > MaxFileSizeBytes)
            {
                yield return new ValidationResult(
                    $"{fieldName}: file must be smaller than 5MB.",
                    new[] { fieldName });
            }
        }
    }
}