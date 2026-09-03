
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels;

public class AdministratorsController : Controller
{
    private readonly Tutor_ManagerDatabaseContext _context;

    public AdministratorsController(Tutor_ManagerDatabaseContext context)
    {
        _context = context;
    }

    // GET: ADMINISTRATORS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Administrators.ToListAsync());
    }

    // GET: /Admin/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var admin = await _context.Administrators
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (admin == null)
            return NotFound();

        bool isAlsoTutor = await _context.Tutors.AnyAsync(t => t.UserId == userId);

        var model = new AdminDashboardViewModel
        {
            FirstName = admin.User.FirstName,
            PendingApplicationsCount = await _context.TutorApplications
                .CountAsync(a => a.Status == Tutor_Manager.Models.Enums.ApplicationStatus.Pending),
            ChangesRequiredCount = await _context.TutorApplications
                .CountAsync(a => a.Status == Tutor_Manager.Models.Enums.ApplicationStatus.ChangesRequired),
            ApprovedTutorsCount = await _context.TutorApplications
                .CountAsync(a => a.Status == Tutor_Manager.Models.Enums.ApplicationStatus.Approved),
            TotalAdmins = await _context.Administrators.CountAsync(),
            IsAlsoTutor = isAlsoTutor
        };

        return View(model);
    }

    // GET: ADMINISTRATORS/Details/5
    public async Task<IActionResult> Details(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var administrator = await _context.Administrators
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (administrator == null)
        {
            return NotFound();
        }

        return View(administrator);
    }

    // GET: ADMINISTRATORS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ADMINISTRATORS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,User,Position")] Administrator administrator)
    {
        if (ModelState.IsValid)
        {
            _context.Add(administrator);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(administrator);
    }

    // GET: ADMINISTRATORS/Edit/5
    public async Task<IActionResult> Edit(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var administrator = await _context.Administrators.FindAsync(userid);
        if (administrator == null)
        {
            return NotFound();
        }
        return View(administrator);
    }

    // POST: ADMINISTRATORS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? userid, [Bind("UserId,User,Position")] Administrator administrator)
    {
        if (userid != administrator.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(administrator);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdministratorExists(administrator.UserId))
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
        return View(administrator);
    }

    // GET: ADMINISTRATORS/Delete/5
    public async Task<IActionResult> Delete(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var administrator = await _context.Administrators
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (administrator == null)
        {
            return NotFound();
        }

        return View(administrator);
    }

    // POST: ADMINISTRATORS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? userid)
    {
        var administrator = await _context.Administrators.FindAsync(userid);
        if (administrator != null)
        {
            _context.Administrators.Remove(administrator);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AdministratorExists(int? userid)
    {
        return _context.Administrators.Any(e => e.UserId == userid);
    }
}
