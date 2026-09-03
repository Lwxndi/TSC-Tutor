// Controllers/AccountActivationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Services.Activation;
using Tutor_Manager.ViewModels;


namespace Tutor_Manager.Controllers
{
    [AllowAnonymous]
    public class AccountActivationController : Controller
    {
        private readonly IAccountActivationService _activationService;

        public AccountActivationController(IAccountActivationService activationService)
        {
            _activationService = activationService;
        }

        // GET: /AccountActivation/Activate?token=xyz
        [HttpGet]
        public async Task<IActionResult> Activate(string token)
        {
            var result = await _activationService.ValidateTokenAsync(token);

            if (!result.IsValid)
            {
                ViewBag.ErrorMessage = result.ErrorMessage;
                return View("ActivationError");
            }

            return View(new SetPasswordViewModel { Token = token });
        }

        // POST: /AccountActivation/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _activationService.ActivateAccountAsync(model.Token, model.Password);

            if (!success)
            {
                ViewBag.ErrorMessage = "This activation link is no longer valid.";
                return View("ActivationError");
            }

            TempData["SuccessMessage"] = "Your account is now active. You can log in.";
            return RedirectToAction("Login", "Home");
        }
    }
}