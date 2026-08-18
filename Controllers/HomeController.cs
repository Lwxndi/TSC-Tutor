using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public HomeController(ILogger<HomeController> logger, Tutor_ManagerDatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/RegisterLearner
        [HttpGet]
        public async Task<IActionResult> RegisterLearner()
        {
            var model = new RegisterLearnerViewModel
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Email = string.Empty,
                PhoneNumber = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                GuardianPhoneNumber1 = string.Empty,
                AvailableSubjects = await _context.Subjects
                    .Select(s => new SubjectSelection
                    {
                        SubjectId = s.SubjectId,
                        SubjectName = s.SubjectName,
                        IsSelected = false
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: /Home/RegisterLearner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterLearner(RegisterLearnerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // AvailableSubjects round-trips through the hidden fields on POST,
                // so there's no need to re-query it before redisplaying the form.
                return View(model);
            }

            // The database enforces a unique Email index, but checking here first
            // gives a friendly validation error instead of a raw SQL exception.
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                AltPhoneNumber = model.AltPhoneNumber,
                Gender = model.Gender,
                PasswordHash = string.Empty // placeholder, set below once we can hash against this instance
            };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

            var learnerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Learner");
            if (learnerRole == null)
            {
                // Roles table needs to be seeded (Tutor/Learner/Parent/Admin) - see note below.
                ModelState.AddModelError(string.Empty, "Registration is temporarily unavailable. Please contact support.");
                return View(model);
            }

            newUser.UserRoles.Add(new UserRole { Role = learnerRole });

            var learner = new Learner
            {
                User = newUser,
                GradeLevel = model.GradeLevel,
                SchoolName = model.SchoolName
            };

            foreach (var subject in model.AvailableSubjects.Where(s => s.IsSelected))
            {
                learner.Subjects.Add(new LearnerSubject { SubjectId = subject.SubjectId });
            }

            // Link guardians by phone number. A number that doesn't match an existing
            // account is currently just skipped - nothing gets created for it.
            // Real gap to decide on: should an unmatched number create a placeholder
            // Parent account, trigger an invite, or block registration entirely?
            var guardianPhones = new[] { model.GuardianPhoneNumber1, model.GuardianPhoneNumber2, model.GuardianPhoneNumber3 }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            bool isFirstGuardian = true;
            foreach (var phone in guardianPhones)
            {
                var guardianUser = await _context.Users
                    .Include(u => u.Parent)
                    .FirstOrDefaultAsync(u => u.PhoneNumber == phone);

                if (guardianUser != null)
                {
                    if (guardianUser.Parent == null)
                    {
                        guardianUser.Parent = new Parent { UserId = guardianUser.UserId };

                        var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Parent");
                        if (parentRole != null)
                        {
                            guardianUser.UserRoles.Add(new UserRole { RoleId = parentRole.RoleId, UserId = guardianUser.UserId });
                        }
                    }

                    learner.Guardians.Add(new LearnerGuardian
                    {
                        Parent = guardianUser.Parent,
                        RelationshipToLearner = model.GuardianRelationship,
                        IsPrimaryContact = isFirstGuardian
                    });
                }

                isFirstGuardian = false;
            }

            var year = DateTime.Now.Year;
            var nextNumber = _context.Learners.Count() + 1;
            learner.TscNumber = $"TSC{year}{nextNumber:D4}"; // TSC20260001

            _context.Learners.Add(learner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction(nameof(Login));
        }


        [HttpGet]
        public IActionResult RegisterGuardian()
        {
            return View(new RegisterGuardianViewModel());
        }

        //[HttpPost]
        //public async Task<IActionResult> RegisterGuardian(RegisterGuardianViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        //    {
        //        ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
        //        return View(model);
        //    }

        //    var user = new User
        //    {
        //        FirstName = model.Name,
        //        LastName = model.Surname,
        //        Email = model.Email,
        //        PhoneNumber = model.PhoneNumber,
        //        PasswordHash = string.Empty 
        //    };
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync(); // need UserId generated before linking below

        //    var guardian = new Parent { UserId = user.UserId };
        //    _context.Parents.Add(guardian);

        //    _context.UserRoles.Add(new UserRole
        //    {
        //        UserId = user.UserId,
        //        Role = "Guardian" // match however you're storing roles right now
        //    });

        //    var learner = await _context.Learners
        //        .FirstOrDefaultAsync(l => l.TscNumber == model.LearnerTscNumber);

        //    if (learner != null)
        //    {
        //        _context.LearnerGuardians.Add(new LearnerGuardian
        //        {
        //            LearnerId = learner.UserId,
        //            GuardianId = guardian.UserId
        //        });
        //    }
        //    // no match -> guardian account still gets created, just unlinked for now

        //    await _context.SaveChangesAsync();

        //    TempData["LinkStatus"] = learner != null
        //        ? "Your account has been linked to your child's profile."
        //        : "We couldn't find a learner with that TSC number. You can try linking again from your dashboard.";

        //    return RedirectToAction("Login");
        //}

        // GET: /Home/Login for all users
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel { Email = string.Empty, Password = string.Empty });
        }

        // POST: /Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // TODO: this confirms the credentials are correct but doesn't actually
            // sign the user in yet - that needs cookie authentication configured in
            // Program.cs (AddAuthentication().AddCookie(), app.UseAuthentication())
            // plus a HttpContext.SignInAsync(...) call here. Flagging rather than
            // guessing at that setup - worth doing as its own dedicated step.
            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}