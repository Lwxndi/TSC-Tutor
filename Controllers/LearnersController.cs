

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Models;
using Tutor_Manager.Services.Email;
using Tutor_Manager.ViewModels;

public class LearnersController : Controller
{
    private readonly Tutor_ManagerDatabaseContext _context;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templates;

    public LearnersController(Tutor_ManagerDatabaseContext context, IEmailService emailService, IEmailTemplateService templates)
    {
        _context = context;
        _emailService = emailService;
        _templates = templates;
    }

    // GET: LEARNERS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Learners.ToListAsync());
    }

    // GET: /Learners/Dashboard
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var learner = await _context.Learners
            .Include(l => l.User)
            .Include(l => l.Subjects)
                .ThenInclude(ls => ls.Subject)
            .Include(l => l.Guardians)
                .ThenInclude(lg => lg.Parent)
                    .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(l => l.UserId == userId);

        if (learner == null)
            return NotFound();

        var model = new LearnerDashboardViewModel
        {
            FirstName = learner.User.FirstName,
            TscNumber = learner.TscNumber,
            GradeLevel = learner.GradeLevel.ToString(),
            SchoolName = learner.SchoolName,
            Subjects = learner.Subjects
                .Select(ls => ls.Subject.SubjectName)
                .ToList(),
            Guardians = learner.Guardians.Select(lg => new GuardianSummary
            {
                FullName = $"{lg.Parent.User.FirstName} {lg.Parent.User.LastName}",
                PhoneNumber = lg.Parent.User.PhoneNumber,
                Relationship = lg.RelationshipToLearner
            }).ToList(),
            UpcomingSessions = new List<string>() // placeholder - wire up once sessions exist
        };

        return View(model);
    }


    // GET: LEARNERS/Details/5
    public async Task<IActionResult> Details(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var learner = await _context.Learners
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (learner == null)
        {
            return NotFound();
        }

        return View(learner);
    }

   
    public IActionResult Create()
    {
        return View();
    }

    // POST: LEARNERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,GradeLevel,SchoolName")] Learner learner)
    {
        if (ModelState.IsValid)
        {
            _context.Add(learner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(learner);
    }

    // GET: LEARNERS/Edit/5
    public async Task<IActionResult> Edit(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var learner = await _context.Learners.FindAsync(userid);
        if (learner == null)
        {
            return NotFound();
        }
        return View(learner);
    }

    // POST: LEARNERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? userid, [Bind("UserId,GradeLevel,SchoolName")] Learner learner)
    {
        if (userid != learner.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(learner);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LearnerExists(learner.UserId))
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
        return View(learner);
    }

    // GET: LEARNERS/Delete/5
    public async Task<IActionResult> Delete(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var learner = await _context.Learners
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (learner == null)
        {
            return NotFound();
        }

        return View(learner);
    }

    // POST: LEARNERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? userid)
    {
        var learner = await _context.Learners.FindAsync(userid);
        if (learner != null)
        {
            _context.Learners.Remove(learner);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LearnerExists(int? userid)
    {
        return _context.Learners.Any(e => e.UserId == userid);
    }
}