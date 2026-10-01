using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Models;
using Tutor_Manager.Services.QuizzServices;

namespace Tutor_Manager.Controllers
{
    [Authorize]
    public class QuizImageController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IQuizImageStorage _images;

        public QuizImageController(Tutor_ManagerDatabaseContext context, IQuizImageStorage images)
        { _context = context; _images = images; }

        [HttpGet]
        public async Task<IActionResult> Image(int questionId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var q = await _context.QuizQuestions
                .Where(x => x.Id == questionId)
                .Select(x => new { x.ImagePath, x.QuizId, x.Quiz.TutorUserId })
                .FirstOrDefaultAsync();

            if (q?.ImagePath == null) return NotFound();

            var allowed = q.TutorUserId == userId
                || await _context.QuizAttempts.AnyAsync(a => a.QuizId == q.QuizId && a.LearnerUserId == userId);
            if (!allowed) return Forbid();

            var path = _images.GetFullPath(q.ImagePath);
            if (!System.IO.File.Exists(path)) return NotFound();

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "private, max-age=3600";
            return PhysicalFile(path, _images.ContentTypeFor(q.ImagePath));
        }
    }
}