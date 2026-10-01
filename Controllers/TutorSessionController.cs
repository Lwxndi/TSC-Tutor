using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services;
using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Tutor")]
    public class TutorSessionController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IAttendanceService _attendanceService;

        public TutorSessionController(ISessionService sessionService, IAttendanceService attendanceService)
        {
            _sessionService = sessionService;
            _attendanceService = attendanceService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> Index(string tab = "today")
        {
            ViewBag.Tab = tab;
            var board = await _sessionService.GetTutorSessionBoardAsync(CurrentUserId);
            return View(board);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int sessionId)
        {
            var (success, error) = await _sessionService.StartSessionAsync(sessionId, CurrentUserId);
            if (!success) TempData["Error"] = error;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int sessionId, string reason)
        {
            var (success, error) = await _sessionService.CancelSessionForTutorAsync(sessionId, CurrentUserId, reason);
            TempData[success ? "Success" : "Error"] = success ? "Session cancelled." : error;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Complete(int sessionId)
        {
            var session = await _sessionService.GetSessionForTutorAsync(sessionId, CurrentUserId);
            if (session == null) return NotFound();

            if (session.Status == SessionStatus.Completed)
            {
                TempData["Error"] = "Session is already completed.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SessionSummary = session;
            ViewBag.UnmarkedCount = await _attendanceService.GetUnmarkedCountAsync(sessionId);
            return View(new CompleteSessionViewModel { SessionId = sessionId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(CompleteSessionViewModel model)
        {
            var (success, error) = await _sessionService.CompleteSessionAsync(model.SessionId, CurrentUserId, model);
            if (!success)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Session completed.";
            return RedirectToAction(nameof(Index));
        }

        // ── Attendance ──

        [HttpGet]
        public async Task<IActionResult> Attendance(int sessionId)
        {
            var vm = await _attendanceService.GetForTutorAsync(sessionId, CurrentUserId);
            if (vm == null) return NotFound();

            if (!vm.CanEdit)
            {
                TempData["Error"] = "Start the session before taking attendance.";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Attendance(MarkAttendanceViewModel model)
        {
            var (success, error) = await _attendanceService.SaveForTutorAsync(model, CurrentUserId);
            if (!success)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Attendance), new { sessionId = model.SessionId });
            }

            TempData["Success"] = "Attendance saved.";
            return RedirectToAction(nameof(Index));
        }
    }
}