using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Tutor_Manager.Models;
using Tutor_Manager.Services.Email;
using Tutor_Manager.Services.Notifications;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templates;
        private readonly INotificationService _notifications;
        public HomeController(ILogger<HomeController> logger, Tutor_ManagerDatabaseContext context, IEmailService emailService, IEmailTemplateService templates, INotificationService notifications)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _templates = templates;
            _notifications = notifications;
        }

        
       
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // TEMPORARY - remove after use
        //[HttpGet]
        ////public IActionResult GenerateHash(string password)
        ////{
        ////    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        ////    var hash = hasher.HashPassword(new User { Email = "temp" }, password);
        ////    return Content(hash);
        ////}

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
                SchoolName = model.SchoolName,
                TscNumber = $"TSC{DateTime.Now.Year}{(_context.Learners.Count() + 1):D4}"
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

            var message = _templates.Build(EmailType.RegistrationConfirmation, newUser.Email, new Dictionary<string, string>
            {
                { "FirstName", newUser.FirstName },
                { "TscNumber", learner.TscNumber }
            });
                        await _emailService.SendAsync(message);


            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction("Dashboard", "Learners");
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

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

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

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                };
            claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.RoleName)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";

            // Priority order for dual-role users (e.g. Michael: Admin + Tutor) — Admin wins,
            // since that's the more privileged/primary context for someone holding both.
            var roleNames = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();

            if (roleNames.Contains("Admin"))
                return RedirectToAction("Dashboard", "Administrators");
            if (roleNames.Contains("Tutor"))
                return RedirectToAction("Dashboard", "Tutors");
            if (roleNames.Contains("Parent"))
                return RedirectToAction("Dashboard", "Parents");
            if (roleNames.Contains("Learner"))
                return RedirectToAction("Dashboard", "Learners");

            return RedirectToAction(nameof(Index));
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var firstName = User.Identity?.Name?.Split(' ').FirstOrDefault();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = string.IsNullOrEmpty(firstName)
                ? "You have been logged out."
                : $"You've been logged out, {firstName}. See you soon!";

            return RedirectToAction(nameof(Index));
        }



        // GET: /Home/RegisterGuardian
        [HttpGet]
        public IActionResult RegisterGuardian()
        {
            return View(new RegisterGuardianViewModel
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Email = string.Empty,
                PhoneNumber = string.Empty,
                AltPhoneNumber = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                LearnerTscNumber = string.Empty,
                RelationshipToLearner = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterGuardian(RegisterGuardianViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
                PasswordHash = string.Empty
            };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

            var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Parent");
            if (parentRole == null)
            {
                ModelState.AddModelError(string.Empty, "Registration is temporarily unavailable. Please contact support.");
                return View(model);
            }

            newUser.UserRoles.Add(new UserRole { Role = parentRole });

            var parent = new Parent { User = newUser };

            var learner = await _context.Learners
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.TscNumber == model.LearnerTscNumber);

            if (learner != null)
            {
                parent.Learners.Add(new LearnerGuardian
                {
                    Learner = learner,
                    RelationshipToLearner = model.RelationshipToLearner,
                    IsPrimaryContact = true
                });
            }

            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            var emailType = learner != null ? EmailType.GuardianLinked : EmailType.GuardianUnlinked;
            var notifType = learner != null ? NotificationType.GuardianLinked : NotificationType.GuardianUnlinked;

            var data = new Dictionary<string, string>
    {
        { "FirstName", newUser.FirstName },
        { "GuardianName", $"{newUser.FirstName} {newUser.LastName}" },
        { "LearnerName", learner != null ? $"{learner.User.FirstName} {learner.User.LastName}" : "" },
        { "TscNumber", model.LearnerTscNumber ?? "" }
    };

            var message = _templates.Build(emailType, newUser.Email, data);
            await _emailService.SendAsync(message);
            await _notifications.SendAsync(newUser.UserId, notifType, data);

            TempData["SuccessMessage"] = learner != null
                ? "Registration successful! Your account has been linked to your child's profile."
                : "Registration successful! We couldn't find a learner with that TSC number - you can try linking again from your dashboard.";

            return RedirectToAction(nameof(Login));
        }
        // POST: /Home/RegisterGuardian
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegisterGuardian(RegisterGuardianViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        //    {
        //        ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
        //        return View(model);
        //    }

        //    var newUser = new User
        //    {
        //        FirstName = model.FirstName,
        //        LastName = model.LastName,
        //        Email = model.Email,
        //        PhoneNumber = model.PhoneNumber,
        //        AltPhoneNumber = model.AltPhoneNumber,
        //        PasswordHash = string.Empty // placeholder, set below once we can hash against this instance
        //    };
        //    newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

        //    var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Parent");
        //    if (parentRole == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "Registration is temporarily unavailable. Please contact support.");
        //        return View(model);
        //    }

        //    newUser.UserRoles.Add(new UserRole { Role = parentRole });

        //    var parent = new Parent
        //    {
        //        User = newUser
        //    };

        //    // Link to their child using the TSC number. No match -> account still gets
        //    // created, just unlinked; they can retry from their dashboard afterward.
        //    var learner = await _context.Learners
        //        .FirstOrDefaultAsync(l => l.TscNumber == model.LearnerTscNumber);

        //    if (learner != null)
        //    {
        //        parent.Learners.Add(new LearnerGuardian
        //        {
        //            Learner = learner,
        //            RelationshipToLearner = model.RelationshipToLearner,
        //            IsPrimaryContact = true
        //        });
        //    }

        //    //var year = DateTime.Now.Year;
        //    //var nextNumber = _context.Learners.Count() + 1;


        //    _context.Parents.Add(parent);
        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = learner != null
        //        ? "Registration successful! Your account has been linked to your child's profile."
        //        : "Registration successful! We couldn't find a learner with that TSC number - you can try linking again from your dashboard.";

        //    return RedirectToAction(nameof(Login));
        //}

        public IActionResult Dashboard()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionFeature?.Error;

            // for now, simplest possible logging — just write to console/output
            Console.WriteLine($"Unhandled error at {exceptionFeature?.Path}: {exception}");
            ViewBag.ErrorMessage = exception?.ToString();

            return View();
            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}