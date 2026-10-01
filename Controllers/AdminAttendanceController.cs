using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Services;
using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AdminAttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Edit(int sessionId)
        {
            var vm = await _attendanceService.GetForAdminAsync(sessionId);
            if (vm == null) return NotFound();

            if (!vm.CanEdit)
            {
                TempData["Error"] = "Attendance opens once the session has started.";
                return RedirectToAction("Index", "Session");
            }

            return View("~/Views/TutorSession/Attendance.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MarkAttendanceViewModel model)
        {
            var (success, error) = await _attendanceService.SaveForAdminAsync(model, CurrentUserId);
            if (!success)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Edit), new { sessionId = model.SessionId });
            }

            TempData["Success"] = "Attendance saved.";
            return RedirectToAction("Index", "Session");
        }
    }
}