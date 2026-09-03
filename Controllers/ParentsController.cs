
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Models;
using Tutor_Manager.Services.Email;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    public class ParentsController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templates;

    public ParentsController(Tutor_ManagerDatabaseContext context, IEmailService emailService, IEmailTemplateService templates)
        {
            _context = context;
        _emailService = emailService;
        _templates = templates;
        }

    // GET: PARENTS
        public async Task<IActionResult> Index()
        {
        return View(await _context.Parents.ToListAsync());
    }


    // GET: /Parents/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var parent = await _context.Parents
            .Include(p => p.User)
            .Include(p => p.Learners).ThenInclude(lg => lg.Learner).ThenInclude(l => l.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (parent == null)
        {
            return NotFound();
        }

        var model = new ParentDashboardViewModel
        {
            FirstName = parent.User.FirstName,
            Learners = parent.Learners.Select(lg => new LinkedLearnerSummary
            {
                FullName = $"{lg.Learner.User.FirstName} {lg.Learner.User.LastName}",
                TscNumber = lg.Learner.TscNumber,
                GradeLevel = lg.Learner.GradeLevel.ToString()
            }).ToList(),
            UpcomingSessions = new List<string>() // placeholder - wire up once sessions exist
        };

        return View(model);
    }

    // POST: /Parents/LinkLearner
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkLearner(string tscNumber)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
        if (parent == null)
        {
            return NotFound();
        }

        var learner = await _context.Learners.FirstOrDefaultAsync(l => l.TscNumber == tscNumber);
        if (learner == null)
        {
            TempData["LinkError"] = "No learner found with that TSC number.";
            return RedirectToAction(nameof(Dashboard));
        }

        bool alreadyLinked = await _context.LearnerGuardians
            .AnyAsync(lg => lg.LearnerUserId == learner.UserId && lg.ParentUserId == parent.UserId);
        if (alreadyLinked)
        {
            TempData["LinkError"] = "This learner is already linked to your account.";
            return RedirectToAction(nameof(Dashboard));
        }

        _context.LearnerGuardians.Add(new LearnerGuardian
        {
            LearnerUserId = learner.UserId,
            ParentUserId = parent.UserId,
            IsPrimaryContact = false
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Learner linked successfully.";
        return RedirectToAction(nameof(Dashboard));
    }



    // GET: PARENTS/Details/5
    public async Task<IActionResult> Details(int? userid)
            {
        if (userid == null)
        {
                return NotFound();
            }

            var parent = await _context.Parents
            .FirstOrDefaultAsync(m => m.UserId == userid);
            if (parent == null)
            {
                return NotFound();
            }

            return View(parent);
        }

    // GET: PARENTS/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

    // POST: PARENTS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,User,Learners")] Parent parent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", parent.UserId);
            return View(parent);
        }

    // GET: PARENTS/Edit/5
    public async Task<IActionResult> Edit(int? userid)
        {
        if (userid == null)
            {
                return NotFound();
            }

        var parent = await _context.Parents.FindAsync(userid);
            if (parent == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", parent.UserId);
            return View(parent);
        }

    // POST: PARENTS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? userid, [Bind("UserId,User,Learners")] Parent parent)
        {
        if (userid != parent.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParentExists(parent.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parent);
        }

    // GET: PARENTS/Delete/5
    public async Task<IActionResult> Delete(int? userid)
        {
        if (userid == null)
            {
                return NotFound();
            }

            var parent = await _context.Parents
            .FirstOrDefaultAsync(m => m.UserId == userid);
            if (parent == null)
            {
                return NotFound();
            }

            return View(parent);
        }

    // POST: PARENTS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? userid)
        {
        var parent = await _context.Parents.FindAsync(userid);
            if (parent != null)
            {
                _context.Parents.Remove(parent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    private bool ParentExists(int? userid)
        {
        return _context.Parents.Any(e => e.UserId == userid);
    }
}
