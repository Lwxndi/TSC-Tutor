using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Services.Upload;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApplicationDocumentController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IApplicationFileStorageService _fileStorage;

        public ApplicationDocumentController(Tutor_ManagerDatabaseContext context, IApplicationFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.ApplicationDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            if (document == null)
                return NotFound();

            var fullPath = _fileStorage.GetFullPath(document.StorageKey);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return File(stream, document.ContentType, document.OriginalFileName);
        }
    }
}