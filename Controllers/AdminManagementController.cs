using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Activation;
using Tutor_Manager.Services.Email;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminManagementController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IAccountActivationService _accountActivationService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public AdminManagementController(
            Tutor_ManagerDatabaseContext context,
            IAccountActivationService accountActivationService,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _context = context;
            _accountActivationService = accountActivationService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        // GET: /AdminManagement/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var admins = await _context.Administrators
                .Include(a => a.User)
                .Select(a => new
                {
                    a.UserId,
                    Name = a.User.FirstName + " " + a.User.LastName,
                    a.User.Email,
                    a.Position,
                    a.User.IsActive
                })
                .ToListAsync();

            return View(admins);
        }

        // GET: /AdminManagement/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterAdminViewModel());
        }

        // POST: /AdminManagement/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.Surname,
                Email = model.Email,
                PhoneNumber = model.Phone,
                PasswordHash = "PENDING_ACTIVATION_" + Guid.NewGuid(),
                DateCreated = DateTime.Now,
                IsActive = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 4 }); // Admin

            _context.Administrators.Add(new Administrator
            {
                UserId = user.UserId,
                Position = model.Position
            });

            await _context.SaveChangesAsync();

            var token = await _accountActivationService.GenerateTokenAsync(user.UserId);
            var activationLink = Url.Action("Activate", "AccountActivation", new { token }, Request.Scheme);

            var email = _emailTemplateService.Build(
                EmailType.AdminAccountCreated,
                user.Email,
                new Dictionary<string, string>
                {
                    { "FirstName", user.FirstName },
                    { "ActivationLink", activationLink ?? string.Empty }
                });

            try
            {
                await _emailService.SendAsync(email);
            }
            catch
            {
                // TODO: log failure — account still exists, don't roll back.
            }

            TempData["SuccessMessage"] = $"Admin account created for {user.FirstName} {user.LastName}. An activation email has been sent.";
            return RedirectToAction("Index");
        }
    }
}