using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Services;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Tutor")]
    public class StudyMaterialController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IStudyMaterialService _studyMaterialService;

        public StudyMaterialController(Tutor_ManagerDatabaseContext context, IStudyMaterialService studyMaterialService)
        {
            _context = context;
            _studyMaterialService = studyMaterialService;
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var model = new StudyMaterialUploadViewModel
            {
                AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(StudyMaterialUploadViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            var result = await _studyMaterialService.CreateAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Study material uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var materials = await _context.StudyMaterials
                .Where(m => m.TutorUserId == tutorUserId && m.IsActive)
                .Include(m => m.Subject)
                .Include(m => m.Files)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            return View(materials);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var material = await _studyMaterialService.GetByIdAsync(id, tutorUserId);

            if (material == null)
                return NotFound();

            var model = new StudyMaterialEditViewModel
            {
                Id = material.Id,
                SubjectId = material.SubjectId,
                Title = material.Title,
                Description = material.Description,
                TopicChapterLabel = material.TopicChapterLabel,
                ExistingFiles = material.Files.Select(f => new ExistingFileViewModel { Id = f.Id, FileName = f.FileName }).ToList(),
                AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudyMaterialEditViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            var result = await _studyMaterialService.UpdateAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Study material updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _studyMaterialService.DeleteAsync(id, tutorUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? "Study material deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _studyMaterialService.BulkDeleteAsync(ids ?? new List<int>(), tutorUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? $"{result.StudyMaterialId} item(s) deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetTutorSubjectsAsync(int tutorUserId)
        {
            return await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .Select(ts => new SelectListItem
                {
                    Value = ts.SubjectId.ToString(),
                    Text = ts.Subject.SubjectName
                })
                .Distinct()
                .OrderBy(s => s.Text)
                .ToListAsync();
        }
    }
}