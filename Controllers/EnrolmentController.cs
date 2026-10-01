using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Services;
using Tutor_Manager.ViewModels;
using Tutor_Manager.ViewModels.EnrolmentViewModels;

namespace Tutor_Manager.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class EnrolmentController : Controller
    {
        private readonly IEnrolmentService _enrolmentService;

        public EnrolmentController(IEnrolmentService enrolmentService)
        {
            _enrolmentService = enrolmentService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _enrolmentService.GetAllEnrolmentsAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Enroll()
        {
            var model = await _enrolmentService.GetEnrollFormDataAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollLearnerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var formData = await _enrolmentService.GetEnrollFormDataAsync();
                model.AvailableLearners = formData.AvailableLearners;
                model.AvailableOfferings = formData.AvailableOfferings;
                return View(model);
            }

            var (success, error, enrolmentId) = await _enrolmentService.EnrollLearnerAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                var formData = await _enrolmentService.GetEnrollFormDataAsync();
                model.AvailableLearners = formData.AvailableLearners;
                model.AvailableOfferings = formData.AvailableOfferings;
                return View(model);
            }

            TempData["Success"] = "Learner enrolled.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Cancel(int id)
        {
            return View(new CancelEnrolmentViewModel { EnrolmentId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelEnrolmentViewModel model)
        {
            var (success, error) = await _enrolmentService.CancelEnrolmentAsync(model.EnrolmentId, model.Reason);

            if (!success)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Enrolment cancelled.";

            return RedirectToAction(nameof(Index));
        }
    }
}