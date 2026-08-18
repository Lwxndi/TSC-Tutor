
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;

public class TutorsController : Controller
{
    private readonly Tutor_ManagerDatabaseContext _context;

    public TutorsController(Tutor_ManagerDatabaseContext context)
    {
        _context = context;
    }

    // GET: TUTORS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Tutors.ToListAsync());
    }

    // GET: TUTORS/Details/5
    public async Task<IActionResult> Details(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var tutor = await _context.Tutors
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (tutor == null)
        {
            return NotFound();
        }

        return View(tutor);
    }

    // GET: TUTORS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TUTORS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,User,Qualification,Bio,VettingStatus,DateApproved,SubjectsTaught")] Tutor tutor)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tutor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tutor);
    }

    // GET: TUTORS/Edit/5
    public async Task<IActionResult> Edit(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var tutor = await _context.Tutors.FindAsync(userid);
        if (tutor == null)
        {
            return NotFound();
        }
        return View(tutor);
    }

    // POST: TUTORS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? userid, [Bind("UserId,User,Qualification,Bio,VettingStatus,DateApproved,SubjectsTaught")] Tutor tutor)
    {
        if (userid != tutor.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tutor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TutorExists(tutor.UserId))
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
        return View(tutor);
    }

    // GET: TUTORS/Delete/5
    public async Task<IActionResult> Delete(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var tutor = await _context.Tutors
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (tutor == null)
        {
            return NotFound();
        }

        return View(tutor);
    }

    // POST: TUTORS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? userid)
    {
        var tutor = await _context.Tutors.FindAsync(userid);
        if (tutor != null)
        {
            _context.Tutors.Remove(tutor);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TutorExists(int? userid)
    {
        return _context.Tutors.Any(e => e.UserId == userid);
    }
}
