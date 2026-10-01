namespace Tutor_Manager.Services.QuizzServices
{
    public interface IQuizImageStorage
    {
        bool IsAllowed(IFormFile file, out string error);
        Task<string> SaveAsync(IFormFile file);
        string SaveBytes(byte[] bytes, string mimeType);
        string GetFullPath(string fileName);
        string ContentTypeFor(string fileName);
        void Delete(string? fileName);
    }

    public class QuizImageStorage : IQuizImageStorage
    {
        private static readonly Dictionary<string, string> Types = new()
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif"
        };
        private const long MaxBytes = 5 * 1024 * 1024;
        private readonly string _root;

        public QuizImageStorage(IWebHostEnvironment env)
        {
            _root = Path.Combine(env.ContentRootPath, "App_Data", "quiz-images");
            Directory.CreateDirectory(_root);
        }

        public bool IsAllowed(IFormFile file, out string error)
        {
            error = "";
            if (file.Length > MaxBytes) { error = "Image must be 5 MB or smaller."; return false; }
            if (!Types.ContainsKey(Path.GetExtension(file.FileName).ToLowerInvariant()))
            { error = "Only PNG, JPG, WEBP or GIF images are allowed."; return false; }
            return true;
        }

        public async Task<string> SaveAsync(IFormFile file)
        {
            var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
            await using var fs = File.Create(Path.Combine(_root, name));
            await file.CopyToAsync(fs);
            return name;
        }

        public string SaveBytes(byte[] bytes, string mimeType)
        {
            var ext = mimeType switch { "image/jpeg" => ".jpg", "image/webp" => ".webp", _ => ".png" };
            var name = Guid.NewGuid().ToString("N") + ext;
            File.WriteAllBytes(Path.Combine(_root, name), bytes);
            return name;
        }

        // GetFileName blocks path traversal
        public string GetFullPath(string fileName) => Path.Combine(_root, Path.GetFileName(fileName));

        public string ContentTypeFor(string fileName) =>
            Types.GetValueOrDefault(Path.GetExtension(fileName).ToLowerInvariant(), "application/octet-stream");

        public void Delete(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            var p = GetFullPath(fileName);
            if (File.Exists(p)) File.Delete(p);
        }
    }
}