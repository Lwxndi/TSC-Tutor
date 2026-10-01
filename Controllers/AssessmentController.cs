// Controllers/AssessmentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tutor_Manager.Services.AssessmentServices;
using Tutor_Manager.ViewModels.AssessmentViewmodels;
using Tutor_Manager.Models;
using Tutor_Manager.Services.AiProviders;
//using Tutor_Manager.Services.FileServices;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Tutor")]
    public class AssessmentController : Controller
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IAssessmentService _assessmentService;
        private readonly IAssessmentGenerationService _generationService;
        private readonly IAssessmentQuestionService _questionService;

        public AssessmentController(
            Tutor_ManagerDatabaseContext context,
            IAssessmentService assessmentService,
            IAssessmentGenerationService generationService,
            IAssessmentQuestionService questionService)
        {
            _context = context;
            _assessmentService = assessmentService;
            _generationService = generationService;
            _questionService = questionService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = new AssessmentCreateViewModel
            {
                AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssessmentCreateViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            var result = await _assessmentService.CreateAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Assessment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var assessments = await _context.Assessments
                .Where(a => a.TutorUserId == tutorUserId && a.IsActive)
                .Include(a => a.Subject)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(assessments);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var assessment = await _assessmentService.GetByIdAsync(id, tutorUserId);

            if (assessment == null)
                return NotFound();

            if (assessment.Status != AssessmentGenerationStatus.NotGenerated)
            {
                TempData["ErrorMessage"] = "This assessment has already been generated and can no longer be edited.";
                return RedirectToAction(nameof(Index));
            }

            if (assessment.OpenAt <= DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "This assessment has already opened and can no longer be edited.";
                return RedirectToAction(nameof(Index));
            }

            var qp = assessment.Files.First(f => f.Role == AssessmentFileRole.QuestionPaper);
            var mg = assessment.Files.First(f => f.Role == AssessmentFileRole.MarkingGuideline);

            var model = new AssessmentEditViewModel
            {
                Id = assessment.Id,
                SubjectId = assessment.SubjectId,
                Title = assessment.Title,
                Description = assessment.Description,
                DueDate = DateOnly.FromDateTime(assessment.DueDate),
                DeclaredTotalMarks = assessment.DeclaredTotalMarks,
                ExistingQuestionPaperFileName = qp.FileName,
                ExistingMarkingGuidelineFileName = mg.FileName,
                // Fallback only — JS overwrites these with the correct local values on load.
                OpenDate = DateOnly.FromDateTime(assessment.OpenAt),
                OpenTime = TimeOnly.FromDateTime(assessment.OpenAt),
                CloseDate = DateOnly.FromDateTime(assessment.CloseAt),
                CloseTime = TimeOnly.FromDateTime(assessment.CloseAt),
                MaxAttempts = assessment.MaxAttempts,
                AudienceType = assessment.AudienceType,
                OfferingId = assessment.OfferingId,
                ExistingLearnerUserIds = assessment.AudienceLearners.Select(al => al.LearnerUserId).ToList(),
                SelectedLearnerUserIds = assessment.AudienceLearners.Select(al => al.LearnerUserId).ToList(),
                AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId)
            };

            // Raw UTC values, exposed to the view so JS can convert them to local for display.
            ViewBag.OpenAtUtc = assessment.OpenAt.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            ViewBag.CloseAtUtc = assessment.CloseAt.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";

            await PopulateDropdownsAsync(model, tutorUserId);
            return View(model);
        }

        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    var assessment = await _assessmentService.GetByIdAsync(id, tutorUserId);

        //    if (assessment == null)
        //        return NotFound();

        //    if (assessment.Status != Tutor_Manager.Models.AssessmentGenerationStatus.NotGenerated)
        //    {
        //        TempData["ErrorMessage"] = "This assessment has already been generated and can no longer be edited.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    var qp = assessment.Files.First(f => f.Role == Tutor_Manager.Models.AssessmentFileRole.QuestionPaper);
        //    var mg = assessment.Files.First(f => f.Role == Tutor_Manager.Models.AssessmentFileRole.MarkingGuideline);

        //    var model = new AssessmentEditViewModel
        //    {
        //        Id = assessment.Id,
        //        SubjectId = assessment.SubjectId,
        //        Title = assessment.Title,
        //        Description = assessment.Description,
        //        DueDate = DateOnly.FromDateTime(assessment.DueDate),
        //        DeclaredTotalMarks = assessment.DeclaredTotalMarks,
        //        ExistingQuestionPaperFileName = qp.FileName,
        //        ExistingMarkingGuidelineFileName = mg.FileName,
        //        AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId)
        //    };

        //    return View(model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AssessmentEditViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            var result = await _assessmentService.UpdateAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                model.AvailableSubjects = await GetTutorSubjectsAsync(tutorUserId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Assessment updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _assessmentService.DeleteAsync(id, tutorUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? "Assessment deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _assessmentService.BulkDeleteAsync(ids ?? new List<int>(), tutorUserId);

            TempData["SuccessMessage"] = result.Succeeded
                ? $"{result.StudyMaterialId} item(s) deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int id)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _generationService.GenerateAsync(id, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Questions extracted successfully."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Review(int assessmentId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var assessment = await _questionService.GetForReviewAsync(assessmentId, tutorUserId);

            if (assessment == null)
                return NotFound();

            var model = new AssessmentReviewViewModel
            {
                AssessmentId = assessment.Id,
                AssessmentTitle = assessment.Title,
                Status = assessment.Status,
                DeclaredTotalMarks = assessment.DeclaredTotalMarks,
                ComputedTotalMarks = assessment.Questions.Sum(q => q.Marks),
                Questions = assessment.Questions.Select(q => new AssessmentQuestionSummaryViewModel
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    DisplayOrder = q.DisplayOrder
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(int assessmentId, int questionId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var question = await _questionService.GetQuestionAsync(questionId, assessmentId, tutorUserId);

            if (question == null)
                return NotFound();

            var model = new AssessmentQuestionEditViewModel
            {
                Id = question.Id,
                AssessmentId = assessmentId,
                QuestionText = question.QuestionText,
                Marks = question.Marks,
                MarkingGuidance = question.MarkingGuidance
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(AssessmentQuestionEditViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
                return View(model);

            var result = await _questionService.UpdateQuestionAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Question updated.";
            return RedirectToAction(nameof(Review), new { assessmentId = model.AssessmentId });
        }

        [HttpGet]
        public IActionResult AddQuestion(int assessmentId)
        {
            var model = new AssessmentQuestionEditViewModel { AssessmentId = assessmentId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(AssessmentQuestionEditViewModel model)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
                return View(model);

            var result = await _questionService.AddQuestionAsync(model, tutorUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Question added.";
            return RedirectToAction(nameof(Review), new { assessmentId = model.AssessmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int assessmentId, int questionId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _questionService.DeleteQuestionAsync(questionId, assessmentId, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Question deleted."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Review), new { assessmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int assessmentId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _questionService.ConfirmAsync(assessmentId, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Assessment confirmed and ready."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Review), new { assessmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unconfirm(int assessmentId)
        {
            var tutorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _questionService.UnconfirmAsync(assessmentId, tutorUserId);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Assessment un-confirmed — you can edit questions again."
                : result.ErrorMessage;

            return RedirectToAction(nameof(Review), new { assessmentId });
        }

        private async Task<List<SelectListItem>> GetTutorSubjectsAsync(int tutorUserId)
        {
            return await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .Select(ts => new SelectListItem
                {
                    Value = ts.SubjectId.ToString(),
                    Text = ts.Subject.SubjectName
                })
                .Distinct()
                .OrderBy(s => s.Text)
                .ToListAsync();
        }


        // Two overloads MUST exist side by side — Create passes AssessmentCreateViewModel,
        // Edit passes AssessmentEditViewModel.
        private async Task PopulateDropdownsAsync(AssessmentCreateViewModel model, int tutorUserId)
        {
            var offerings = await _context.Offerings
                .Where(o => o.TutorUserId == tutorUserId && o.IsActive)
                .Select(o => new { o.OfferingId, o.SubjectId, o.Subject.SubjectName, o.Grade, o.Type })
                .ToListAsync();
            ViewBag.OfferingsWithSubject = offerings;

            var tutorSubjectIds = await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .Select(ts => ts.SubjectId)
                .Distinct()
                .ToListAsync();

            var learners = await _context.LearnerSubjects
                .Where(ls => tutorSubjectIds.Contains(ls.SubjectId))
                .Select(ls => new
                {
                    ls.LearnerUserId,
                    ls.SubjectId,
                    FullName = ls.Learner.User.FirstName + " " + ls.Learner.User.LastName
                })
                .Distinct()
                .ToListAsync();
            ViewBag.LearnersWithSubject = learners;
        }

        private async Task PopulateDropdownsAsync(AssessmentEditViewModel model, int tutorUserId)
        {
            var offerings = await _context.Offerings
                .Where(o => o.TutorUserId == tutorUserId && o.IsActive)
                .Select(o => new { o.OfferingId, o.SubjectId, o.Subject.SubjectName, o.Grade, o.Type })
                .ToListAsync();
            ViewBag.OfferingsWithSubject = offerings;

            var tutorSubjectIds = await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .Select(ts => ts.SubjectId)
                .Distinct()
                .ToListAsync();

            var learners = await _context.LearnerSubjects
                .Where(ls => tutorSubjectIds.Contains(ls.SubjectId))
                .Select(ls => new
                {
                    ls.LearnerUserId,
                    ls.SubjectId,
                    FullName = ls.Learner.User.FirstName + " " + ls.Learner.User.LastName
                })
                .Distinct()
                .ToListAsync();
            ViewBag.LearnersWithSubject = learners;
        }
    }
}