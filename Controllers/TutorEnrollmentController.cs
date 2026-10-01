using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Services.EnrollmentServices;

namespace Tutor_Manager.Controllers
{
    // Spec §27 — Tutor is view-only: sees their own Offerings' Active roster and each
    // learner's DeliveryMode. No write access to enrollment, no payment visibility at all.
    [Authorize(Roles = "Tutor")]
    public class TutorEnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly Tutor_ManagerDatabaseContext _context;

        public TutorEnrollmentController(IEnrollmentService enrollmentService, Tutor_ManagerDatabaseContext context)
        {
            _enrollmentService = enrollmentService;
            _context = context;
        }

        private int TutorUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Lists this tutor's own Offerings so they can pick which class's roster to view.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var offerings = await _context.Offerings
                .Include(o => o.Subject)
                .Where(o => o.TutorUserId == TutorUserId && o.IsActive)
                .OrderBy(o => o.Subject.SubjectName)
                .ToListAsync();

            var rows = new List<(Models.Offering Offering, int Taken, int Capacity)>();
            foreach (var offering in offerings)
            {
                var (taken, capacity) = await _enrollmentService.GetOfferingOccupancyAsync(offering.OfferingId);
                rows.Add((offering, taken, capacity));
            }

            return View(rows);
        }

        // The actual roster — Active learners only, per spec §16.3 / §27.1. Ownership check
        // is the whole point of this action existing separately from a generic "get roster
        // by id": without it, any tutor could view another tutor's roster by guessing an id.
        [HttpGet]
        public async Task<IActionResult> Roster(int offeringId)
        {
            var offering = await _context.Offerings
                .Include(o => o.Subject)
                .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

            if (offering == null)
                return NotFound();

            if (offering.TutorUserId != TutorUserId)
                return NotFound(); // hide existence, same pattern as TutorSessionController.Complete

            ViewBag.Offering = offering;
            var roster = await _enrollmentService.GetActiveRosterForOfferingAsync(offeringId);
            return View(roster);
        }
    }
}