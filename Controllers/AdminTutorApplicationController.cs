using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Email;
using Tutor_Manager.Services.Activation;
using Tutor_Manager.ViewModels.TutorApplication;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTutorApplicationController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IAccountActivationService _accountActivationService;

        public AdminTutorApplicationController(Tutor_ManagerDatabaseContext context, IEmailService emailService, IEmailTemplateService emailTemplateService, IAccountActivationService accountActivationService)
        {
            _context = context;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _accountActivationService = accountActivationService;
        }

        // GET: /AdminTutorApplication/Index?status=Pending
        [HttpGet]
        public async Task<IActionResult> Index(ApplicationStatus? status)
        {
            var query = _context.TutorApplications.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var applications = await query
                .OrderByDescending(a => a.DateApplied ?? a.DateCreated)
                .Select(a => new AdminApplicationListItemViewModel
                {
                    ApplicationId = a.ApplicationId,
                    ReferenceNumber = a.ReferenceNumber,
                    FirstName = a.FirstName,
                    Surname = a.Surname,
                    Email = a.Email,
                    LocationPreference = a.LocationPreference,
                    Status = a.Status,
                    DateApplied = a.DateApplied
                })
                .ToListAsync();

            var viewModel = new AdminApplicationListViewModel
            {
                Applications = applications,
                StatusFilter = status
            };

            return View(viewModel);
        }

        // GET: /AdminTutorApplication/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var application = await _context.TutorApplications
                .Include(a => a.Qualifications)
                .Include(a => a.Subjects)
                    .ThenInclude(s => s.Subject)
                .Include(a => a.Experience)
                .Include(a => a.Documents)
                .Include(a => a.ReviewedByAdmin)
                .Include(a => a.CreatedTutor)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var viewModel = new AdminApplicationDetailViewModel
            {
                ApplicationId = application.ApplicationId,
                ReferenceNumber = application.ReferenceNumber,
                Status = application.Status,

                PersonalInfo = new TutorApplicationPersonalInfoViewModel
                {
                    ApplicationId = application.ApplicationId,
                    ReferenceNumber = application.ReferenceNumber,
                    FirstName = application.FirstName,
                    Surname = application.Surname,
                    Email = application.Email,
                    Phone = application.Phone,
                    AltPhone = application.AltPhone,
                    LocationPreference = application.LocationPreference,
                    AreaCity = application.AreaCity
                },

                AcademicInfo = new TutorApplicationAcademicInfoViewModel
                {
                    ApplicationId = application.ApplicationId,
                    Qualifications = application.Qualifications.Select(q => new TutorApplicationQualificationEntryViewModel
                    {
                        QualificationType = q.QualificationType,
                        Institution = q.Institution,
                        FieldOfStudy = q.FieldOfStudy,
                        YearCompleted = q.YearCompleted,
                        StudyStatus = q.StudyStatus
                    }).ToList()
                },

                Subjects = application.Subjects.Select(s => new TutorApplicationSubjectRowViewModel
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.Subject.SubjectName,
                    Selected = true,
                    GradeLevels = s.GradeLevels.Split(',').ToList()
                }).ToList(),

                Experience = application.Experience.Select(e => new TutorApplicationExperienceEntryViewModel
                {
                    Institution = e.Institution,
                    SubjectsTaught = e.SubjectsTaught,
                    GradeLevels = e.GradeLevels,
                    Duration = e.Duration,
                    Responsibilities = e.Responsibilities,
                    Achievements = e.Achievements
                }).ToList(),

                SelectedStrengths = string.IsNullOrEmpty(application.StrengthsSelected)
                    ? new List<string>()
                    : application.StrengthsSelected.Split(',').ToList(),
                StrengthsNote = application.StrengthsNote,

                Documents = application.Documents.Select(d => (
                    DocumentType: d.DocumentType.ToString(),
                    FileName: d.OriginalFileName,
                    DownloadUrl: Url.Action("Download", "ApplicationDocument", new { id = d.DocumentId }) ?? string.Empty
                )).ToList(),

                ConsentGiven = application.ConsentGiven,
                ConsentDate = application.ConsentDate,

                DateApplied = application.DateApplied,
                DateReviewed = application.DateReviewed,
                ReviewedByAdminName = application.ReviewedByAdmin != null
                    ? $"{application.ReviewedByAdmin.FirstName} {application.ReviewedByAdmin.LastName}"
                    : null,

                CreatedTutorId = application.CreatedTutorId,
                CreatedTutorNumber = application.CreatedTutor?.TutorNumber
            };

            return View(viewModel);
        }

        // GET: /AdminTutorApplication/Decision/5
        [HttpGet]
        public async Task<IActionResult> Decision(int id)
        {
            var application = await _context.TutorApplications.FindAsync(id);

            if (application == null)
                return NotFound();

            var viewModel = new AdminApplicationDecisionViewModel
            {
                ApplicationId = application.ApplicationId
            };

            return View(viewModel);
        }

        // POST: /AdminTutorApplication/Decision
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decision(AdminApplicationDecisionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var application = await _context.TutorApplications
                .Include(a => a.Subjects)
                .FirstOrDefaultAsync(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
                return NotFound();

            // Application cannot be re-decided once processed, without explicit override
            if (application.Status == ApplicationStatus.Approved || application.Status == ApplicationStatus.Rejected)
            {
                ModelState.AddModelError(string.Empty, "This application has already been decided and cannot be processed again.");
                return View(model);
            }

            var adminUserId = GetCurrentAdminUserId();

            switch (model.Decision)
            {
                case ApplicationDecision.Reject:
                    application.Status = ApplicationStatus.Rejected;
                    application.DateReviewed = DateTime.Now;
                    application.ReviewedByAdminId = adminUserId;
                    await _context.SaveChangesAsync();

                    var rejectEmail = _emailTemplateService.Build(
                        EmailType.TutorApplicationRejected,
                        application.Email,
                        new Dictionary<string, string>
                        {
                            { "FirstName", application.FirstName },
                            { "ReferenceNumber", application.ReferenceNumber }
                        });
                    await _emailService.SendAsync(rejectEmail);
                    break;

                case ApplicationDecision.RequestChanges:
                    application.Status = ApplicationStatus.ChangesRequired;
                    application.DateReviewed = DateTime.Now;
                    application.ReviewedByAdminId = adminUserId;
                    await _context.SaveChangesAsync();

                    var changesEmail = _emailTemplateService.Build(
                        EmailType.TutorApplicationChangesRequired,
                        application.Email,
                        new Dictionary<string, string>
                        {
                            { "FirstName", application.FirstName },
                            { "ReferenceNumber", application.ReferenceNumber },
                            { "Notes", model.ChangesRequiredNotes ?? string.Empty }
                        });
                    await _emailService.SendAsync(changesEmail);
                    break;

                case ApplicationDecision.Approve:
                    await ApproveApplicationAsync(application, adminUserId);
                    break;
            }

            return RedirectToAction("Details", new { id = application.ApplicationId });
        }

        private async Task ApproveApplicationAsync(TutorApplication application, int adminUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            User user;
            Tutor tutor;

            try
            {
                user = new User
                {
                    FirstName = application.FirstName,
                    LastName = application.Surname,
                    Email = application.Email,
                    PhoneNumber = application.Phone,
                    PasswordHash = "PENDING_ACTIVATION_" + Guid.NewGuid(),
                    DateCreated = DateTime.Now,
                    IsActive = false
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 2 });

                var tutorNumber = await GenerateTutorNumberAsync();

                // Qualification is now sourced from the applicant's highest-ranked
                // TutorApplicationQualification entry, since the old single
                // HighestQualification field on TutorApplication no longer exists.
                // Simplest reasonable pick for now: the most recently completed one,
                // falling back to the first entry if none have a YearCompleted.
                var topQualification = application.Qualifications
                    .OrderByDescending(q => q.YearCompleted ?? 0)
                    .FirstOrDefault();

                tutor = new Tutor
                {
                    UserId = user.UserId,
                    Qualification = topQualification?.QualificationType,
                    VettingStatus = "Approved",
                    DateApproved = DateTime.Now,
                    AccountStatus = AccountStatus.PendingActivation,
                    TutorNumber = tutorNumber
                };
                _context.Tutors.Add(tutor);
                await _context.SaveChangesAsync();

                //foreach (var appSubject in application.Subjects)
                //{
                //    var grades = appSubject.GradeLevels
                //        .Split(',')
                //        .Select(g => g.Trim())
                //        .Where(g => byte.TryParse(g, out _))
                //        .Select(byte.Parse);

                //    foreach (var grade in grades)
                //    {
                //        _context.TutorSubjects.Add(new TutorSubject
                //        {
                //            TutorUserId = tutor.UserId,
                //            SubjectId = appSubject.SubjectId,
                //            GradeLevel = (Grade)grade
                //        });
                //    }
                //} MIGHT DELETE CAUSE IT LOOKS LIKE ITS NOT WORKING 


                application.CreatedTutorId = tutor.UserId;
                application.Status = ApplicationStatus.Approved;
                application.DateReviewed = DateTime.Now;
                application.ReviewedByAdminId = adminUserId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // --- Everything below happens AFTER the transaction has committed ---

            var token = await _accountActivationService.GenerateTokenAsync(user.UserId);
            var activationLink = Url.Action("Activate", "AccountActivation", new { token }, Request.Scheme);

            var approveEmail = _emailTemplateService.Build(
                EmailType.TutorApplicationApproved,
                application.Email,
                new Dictionary<string, string>
                {
                    { "FirstName", application.FirstName },
                    { "ReferenceNumber", application.ReferenceNumber },
                    { "TutorNumber", tutor.TutorNumber ?? string.Empty },
                    { "ActivationLink", activationLink ?? string.Empty }
                });

            try
            {
                await _emailService.SendAsync(approveEmail);
            }
            catch
            {
                // Per design doc: email failure must not roll back the already-committed
                // account creation. Log it and move on.
                // TODO: wire into your logging setup once confirmed.
            }
        }

        private async Task<string> GenerateTutorNumberAsync()
        {
            var prefix = "TSCT";

            var lastNumber = await _context.Tutors
                .Where(t => t.TutorNumber != null && t.TutorNumber.StartsWith(prefix))
                .Select(t => t.TutorNumber)
                .OrderByDescending(n => n)
                .FirstOrDefaultAsync();

            int nextSequence = 1;

            if (lastNumber != null)
            {
                var sequencePart = lastNumber!.Substring(prefix.Length);
                if (int.TryParse(sequencePart, out int lastSequence))
                {
                    nextSequence = lastSequence + 1;
                }
            }

            return $"{prefix}{nextSequence:D8}";
        }

        private int GetCurrentAdminUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}