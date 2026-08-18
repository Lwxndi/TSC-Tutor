
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Tutor_Manager.Models;

//public class LearnersController : Controller
//{
//    private readonly Tutor_ManagerDatabaseContext _context;

//    public LearnersController(Tutor_ManagerDatabaseContext context)
//    {
//        _context = context;
//    }

//    // GET: LEARNERS
//    public async Task<IActionResult> Index()    
//    {
//        return View(await _context.Learners.ToListAsync());
//    }

//    // GET: LEARNERS/Details/5
//    public async Task<IActionResult> Details(int? userid)
//    {
//        if (userid == null)
//        {
//            return NotFound();
//        }

//        var learner = await _context.Learners
//            .FirstOrDefaultAsync(m => m.UserId == userid);
//        if (learner == null)
//        {
//            return NotFound();
//        }

//        return View(learner);
//    }

//    // GET: LEARNERS/Create
//    public IActionResult Create()
//    {
//        return View();
//    }

//    // POST: LEARNERS/Create
//    // To protect from overposting attacks, enable the specific properties you want to bind to.
//    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Create([Bind("UserId,User,GradeLevel,SchoolName,Guardians")] Learner learner)
//    {
//        if (ModelState.IsValid)
//        {
//            _context.Add(learner);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }
//        return View(learner);
//    }

//    // GET: LEARNERS/Edit/5
//    public async Task<IActionResult> Edit(int? userid)
//    {
//        if (userid == null)
//        {
//            return NotFound();
//        }

//        var learner = await _context.Learners.FindAsync(userid);
//        if (learner == null)
//        {
//            return NotFound();
//        }
//        return View(learner);
//    }

//    // POST: LEARNERS/Edit/5
//    // To protect from overposting attacks, enable the specific properties you want to bind to.
//    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Edit(int? userid, [Bind("UserId,User,GradeLevel,SchoolName,Guardians")] Learner learner)
//    {
//        if (userid != learner.UserId)
//        {
//            return NotFound();
//        }

//        if (ModelState.IsValid)
//        {
//            try
//            {
//                _context.Update(learner);
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!LearnerExists(learner.UserId))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }
//            return RedirectToAction(nameof(Index));
//        }
//        return View(learner);
//    }

//    // GET: LEARNERS/Delete/5
//    public async Task<IActionResult> Delete(int? userid)
//    {
//        if (userid == null)
//        {
//            return NotFound();
//        }

//        var learner = await _context.Learners
//            .FirstOrDefaultAsync(m => m.UserId == userid);
//        if (learner == null)
//        {
//            return NotFound();
//        }

//        return View(learner);
//    }

//    // POST: LEARNERS/Delete/5
//    [HttpPost, ActionName("Delete")]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> DeleteConfirmed(int? userid)
//    {
//        var learner = await _context.Learners.FindAsync(userid);
//        if (learner != null)
//        {
//            _context.Learners.Remove(learner);
//        }

//        await _context.SaveChangesAsync();
//        return RedirectToAction(nameof(Index));
//    }

//    private bool LearnerExists(int? userid)
//    {
//        return _context.Learners.Any(e => e.UserId == userid);
//    }
//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;

public class LearnersController : Controller
{
    private readonly Tutor_ManagerDatabaseContext _context;

    public LearnersController(Tutor_ManagerDatabaseContext context)
    {
        _context = context;
    }

    // GET: LEARNERS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Learners.ToListAsync());
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

    // GET: LEARNERS/Create
    // NOTE: real Learner sign-up happens through HomeController.RegisterLearner,
    // which creates the User + Learner + Subjects + Guardians together. This
    // Create action only makes sense for an ADMIN attaching a Learner profile
    // to a User that already exists (e.g. fixing a bad registration) - it does
    // not currently offer a way to pick which User, which you'd want to add
    // (a dropdown of Users without an existing Learner profile) before using this.
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