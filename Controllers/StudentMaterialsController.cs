// Controllers/StudentMaterialsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Learner")]
    public class StudentMaterialsController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;

        public StudentMaterialsController(Tutor_ManagerDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var studentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var enrolledSubjectIds = await _context.LearnerSubjects
                .Where(ls => ls.LearnerUserId == studentUserId)
                .Select(ls => ls.SubjectId)
                .Distinct()
                .ToListAsync();

            var materials = await _context.StudyMaterials
                .Where(m => enrolledSubjectIds.Contains(m.SubjectId) && m.IsActive)
                .Include(m => m.Subject)
                .Include(m => m.Files)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            return View(materials);
        }
    }
}