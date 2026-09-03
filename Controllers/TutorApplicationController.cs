using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Upload;
using Tutor_Manager.ViewModels;
using Tutor_Manager.ViewModels.TutorApplication;

namespace Tutor_Manager.Controllers
{
    [AllowAnonymous]
    public class TutorApplicationController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IApplicationFileStorageService _fileStorage;

        public TutorApplicationController(Tutor_ManagerDatabaseContext context, IApplicationFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        // GET: /TutorApplication/PersonalInfo
        [HttpGet]
        public async Task<IActionResult> PersonalInfo(int? id)
        {
            if (id == null)
            {
                return View(new TutorApplicationPersonalInfoViewModel());
            }

            var application = await _context.TutorApplications.FindAsync(id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationPersonalInfoViewModel
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
            };

            return View(viewModel);
        }

        // POST: /TutorApplication/PersonalInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersonalInfo(TutorApplicationPersonalInfoViewModel model)
        {
            bool isSkip = Request.Form["isSkip"] == "true";

            if (!isSkip && !ModelState.IsValid)
                return View(model);

            TutorApplication? application;

            if (model.ApplicationId == null)
            {
                application = new TutorApplication
                {
                    ReferenceNumber = await GenerateReferenceNumberAsync(),
                    FirstName = model.FirstName,
                    Surname = model.Surname,
                    Email = model.Email,
                    Phone = model.Phone,
                    AltPhone = model.AltPhone,
                    LocationPreference = model.LocationPreference,
                    AreaCity = model.AreaCity,
                    Status = ApplicationStatus.Draft,
                    DateCreated = DateTime.Now
                };

                _context.TutorApplications.Add(application);
            }
            else
            {
                application = await _context.TutorApplications.FindAsync(model.ApplicationId);

                if (application == null)
                    return NotFound();

                application.FirstName = model.FirstName;
                application.Surname = model.Surname;
                application.Email = model.Email;
                application.Phone = model.Phone;
                application.AltPhone = model.AltPhone;
                application.LocationPreference = model.LocationPreference;
                application.AreaCity = model.AreaCity;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("AcademicInfo", new { id = application.ApplicationId });
        }

        private async Task<string> GenerateReferenceNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"TSC-TA-{year}-";

            var lastNumber = await _context.TutorApplications
                .Where(a => a.ReferenceNumber.StartsWith(prefix))
                .Select(a => a.ReferenceNumber)
                .OrderByDescending(r => r)
                .FirstOrDefaultAsync();

            int nextSequence = 1;

            if (lastNumber != null)
            {
                var lastSequencePart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequencePart, out int lastSequence))
                {
                    nextSequence = lastSequence + 1;
                }
            }

            return $"{prefix}{nextSequence:D5}";
        }

        // GET: /TutorApplication/AcademicInfo?id=5
        [HttpGet]
        public async Task<IActionResult> AcademicInfo(int id)
        {
            var application = await _context.TutorApplications
                .Include(a => a.Qualifications)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationAcademicInfoViewModel
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
            };

            return View(viewModel);
        }

        // POST: /TutorApplication/AcademicInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcademicInfo(TutorApplicationAcademicInfoViewModel model)
        {
            bool isSkip = Request.Form["isSkip"] == "true";

            if (!isSkip && !ModelState.IsValid)
                return View(model);

            var application = await _context.TutorApplications
                .Include(a => a.Qualifications)
                .FirstOrDefaultAsync(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
                return NotFound();

            _context.TutorApplicationQualifications.RemoveRange(application.Qualifications);

            foreach (var q in model.Qualifications)
            {
                if (string.IsNullOrWhiteSpace(q.QualificationType)) continue;

                _context.TutorApplicationQualifications.Add(new TutorApplicationQualification
                {
                    ApplicationId = application.ApplicationId,
                    QualificationType = q.QualificationType,
                    Institution = q.Institution,
                    FieldOfStudy = q.FieldOfStudy,
                    YearCompleted = q.YearCompleted,
                    StudyStatus = q.StudyStatus
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Subjects", new { id = application.ApplicationId });
        }

        // GET: /TutorApplication/Subjects?id=5
        [HttpGet]
        public async Task<IActionResult> Subjects(int id)
        {
            var application = await _context.TutorApplications
                .Include(a => a.Subjects)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var allSubjects = await _context.Subjects.ToListAsync();

            var viewModel = new TutorApplicationSubjectsViewModel
            {
                ApplicationId = application.ApplicationId,
                Subjects = allSubjects.Select(subject =>
                {
                    var existing = application.Subjects
                        .FirstOrDefault(s => s.SubjectId == subject.SubjectId);

                    return new TutorApplicationSubjectRowViewModel
                    {
                        SubjectId = subject.SubjectId,
                        SubjectName = subject.SubjectName,
                        Selected = existing != null,
                        GradeLevels = existing != null
                            ? existing.GradeLevels.Split(',').ToList()
                            : new List<string>()
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /TutorApplication/Subjects
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subjects(TutorApplicationSubjectsViewModel model)
        {
            bool isSkip = Request.Form["isSkip"] == "true";

            if (!isSkip && !ModelState.IsValid)
                return View(model);

            var application = await _context.TutorApplications
                .Include(a => a.Subjects)
                .FirstOrDefaultAsync(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
                return NotFound();

            _context.TutorApplicationSubjects.RemoveRange(application.Subjects);

            var selectedRows = model.Subjects.Where(s => s.Selected).ToList();

            foreach (var row in selectedRows)
            {
                _context.TutorApplicationSubjects.Add(new TutorApplicationSubject
                {
                    ApplicationId = application.ApplicationId,
                    SubjectId = row.SubjectId,
                    GradeLevels = string.Join(",", row.GradeLevels)
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Experience", new { id = application.ApplicationId });
        }

        // GET: /TutorApplication/Experience?id=5
        [HttpGet]
        public async Task<IActionResult> Experience(int id)
        {
            var application = await _context.TutorApplications
                .Include(a => a.Experience)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationExperienceViewModel
            {
                ApplicationId = application.ApplicationId,
                HasExperience = application.Experience.Any(),
                Entries = application.Experience.Select(e => new TutorApplicationExperienceEntryViewModel
                {
                    Institution = e.Institution,
                    SubjectsTaught = e.SubjectsTaught,
                    GradeLevels = e.GradeLevels,
                    Duration = e.Duration,
                    Responsibilities = e.Responsibilities,
                    Achievements = e.Achievements
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /TutorApplication/Experience
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Experience(TutorApplicationExperienceViewModel model)
        {
            bool isSkip = Request.Form["isSkip"] == "true";

            if (!isSkip && !ModelState.IsValid)
                return View(model);

            var application = await _context.TutorApplications
                .Include(a => a.Experience)
                .FirstOrDefaultAsync(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
                return NotFound();

            _context.TutorApplicationExperiences.RemoveRange(application.Experience);

            if (model.HasExperience)
            {
                foreach (var entry in model.Entries)
                {
                    _context.TutorApplicationExperiences.Add(new TutorApplicationExperience
                    {
                        ApplicationId = application.ApplicationId,
                        Institution = entry.Institution,
                        SubjectsTaught = entry.SubjectsTaught,
                        GradeLevels = entry.GradeLevels,
                        Duration = entry.Duration,
                        Responsibilities = entry.Responsibilities,
                        Achievements = entry.Achievements
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Strengths", new { id = application.ApplicationId });
        }

        // GET: /TutorApplication/Strengths?id=5
        [HttpGet]
        public async Task<IActionResult> Strengths(int id)
        {
            var application = await _context.TutorApplications.FindAsync(id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationStrengthsViewModel
            {
                ApplicationId = application.ApplicationId,
                SelectedStrengths = string.IsNullOrEmpty(application.StrengthsSelected)
                    ? new List<string>()
                    : application.StrengthsSelected.Split(',').ToList(),
                StrengthsNote = application.StrengthsNote
            };

            return View(viewModel);
        }

        // POST: /TutorApplication/Strengths
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Strengths(TutorApplicationStrengthsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var application = await _context.TutorApplications.FindAsync(model.ApplicationId);

            if (application == null)
                return NotFound();

            application.StrengthsSelected = string.Join(",", model.SelectedStrengths);
            application.StrengthsNote = model.StrengthsNote;

            await _context.SaveChangesAsync();

            return RedirectToAction("Documents", new { id = application.ApplicationId });
        }

        // GET: /TutorApplication/Documents?id=5
        [HttpGet]
        public async Task<IActionResult> Documents(int id)
        {
            var application = await _context.TutorApplications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationDocumentUploadViewModel
            {
                ApplicationId = application.ApplicationId,
                AlreadyUploadedFileNames = application.Documents
                    .ToDictionary(d => d.DocumentType.ToString(), d => d.OriginalFileName)
            };

            return View(viewModel);
        }

        // POST: /TutorApplication/Documents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Documents(TutorApplicationDocumentUploadViewModel model)
        {
            bool isSkip = Request.Form["isSkip"] == "true";

            var application = await _context.TutorApplications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
                return NotFound();

            model.AlreadyUploadedFileNames = application.Documents
                .ToDictionary(d => d.DocumentType.ToString(), d => d.OriginalFileName);

            if (!isSkip && !ModelState.IsValid)
                return View(model);

            await SaveDocumentIfProvided(application, model.CV, ApplicationDocumentType.CV);
            await SaveDocumentIfProvided(application, model.Transcript, ApplicationDocumentType.Transcript);
            await SaveDocumentIfProvided(application, model.Certificate, ApplicationDocumentType.Certificate);
            await SaveDocumentIfProvided(application, model.TeachingCertificate, ApplicationDocumentType.TeachingCertificate);
            await SaveDocumentIfProvided(application, model.ReferenceLetter, ApplicationDocumentType.ReferenceLetter);
            await SaveDocumentIfProvided(application, model.Other, ApplicationDocumentType.Other);

            await _context.SaveChangesAsync();

            return RedirectToAction("Review", new { id = application.ApplicationId });
        }

        private async Task SaveDocumentIfProvided(TutorApplication application, IFormFile? file, ApplicationDocumentType documentType)
        {
            if (file == null)
                return;

            var storageKey = await _fileStorage.SaveFileAsync(file, application.ReferenceNumber);

            var existing = application.Documents.FirstOrDefault(d => d.DocumentType == documentType);
            if (existing != null)
            {
                _context.ApplicationDocuments.Remove(existing);
            }

            _context.ApplicationDocuments.Add(new ApplicationDocument
            {
                ApplicationId = application.ApplicationId,
                DocumentType = documentType,
                OriginalFileName = file.FileName,
                StorageKey = storageKey,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadDate = DateTime.Now
            });
        }

        // GET: /TutorApplication/Review?id=5
        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var application = await _context.TutorApplications
                .Include(a => a.Qualifications)
                .Include(a => a.Subjects)
                    .ThenInclude(s => s.Subject)
                .Include(a => a.Experience)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationReviewViewModel
            {
                ApplicationId = application.ApplicationId,
                ReferenceNumber = application.ReferenceNumber,

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

                SelectedSubjects = application.Subjects.Select(s => new TutorApplicationSubjectRowViewModel
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

                UploadedDocumentNames = application.Documents
                    .Select(d => $"{d.DocumentType}: {d.OriginalFileName}")
                    .ToList(),

                ConsentGiven = application.ConsentGiven
            };

            return View(viewModel);
        }

        // GET: /TutorApplication/Submit?id=5
        [HttpGet]
        public async Task<IActionResult> Submit(int id)
        {
            var application = await _context.TutorApplications.FindAsync(id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationSubmitViewModel
            {
                ApplicationId = application.ApplicationId
            };

            return View(viewModel);
        }

        // POST: /TutorApplication/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TutorApplicationSubmitViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var application = await _context.TutorApplications
                .Include(a => a.Qualifications)
                .Include(a => a.Subjects)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
                return NotFound();

            var validationErrors = ValidateApplicationForSubmission(application);

            if (validationErrors.Any())
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            application.ConsentGiven = model.ConsentGiven;
            application.ConsentDate = DateTime.Now;
            application.ConsentVersion = model.ConsentVersion;
            application.Status = ApplicationStatus.Pending;
            application.DateApplied = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", new { id = application.ApplicationId });
        }

        private List<string> ValidateApplicationForSubmission(TutorApplication application)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(application.FirstName) ||
                string.IsNullOrWhiteSpace(application.Surname) ||
                string.IsNullOrWhiteSpace(application.Email) ||
                string.IsNullOrWhiteSpace(application.Phone) ||
                string.IsNullOrWhiteSpace(application.AreaCity))
            {
                errors.Add("Personal information is incomplete.");
            }

            if (!application.Qualifications.Any())
            {
                errors.Add("Add at least one academic qualification.");
            }

            if (!application.Subjects.Any())
            {
                errors.Add("Select at least one subject you're applying to teach.");
            }

            if (application.Subjects.Any(s => string.IsNullOrWhiteSpace(s.GradeLevels)))
            {
                errors.Add("Every selected subject must have at least one grade level.");
            }

            if (!application.Documents.Any(d => d.DocumentType == ApplicationDocumentType.CV))
            {
                errors.Add("A CV is required before submitting.");
            }

            if (!application.Documents.Any(d => d.DocumentType == ApplicationDocumentType.Transcript))
            {
                errors.Add("An academic transcript is required before submitting.");
            }

            return errors;
        }

        // GET: /TutorApplication/Confirmation?id=5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var application = await _context.TutorApplications.FindAsync(id);

            if (application == null)
                return NotFound();

            var viewModel = new TutorApplicationConfirmationViewModel
            {
                ReferenceNumber = application.ReferenceNumber
            };

            return View(viewModel);
        }
    }
}