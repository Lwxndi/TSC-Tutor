using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Services.PaymentServices;
using Tutor_Manager.Services.ReceiptServices;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    // Spec §18.2/§29.2 — Admin has visibility and exception-handling only; never manually marks
    // anything as paid. This controller has no such action and none should be added.
    [Authorize(Roles = "Admin")]
    public class AdminPaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IReceiptService _receiptService;

        public AdminPaymentController(IPaymentService paymentService, IReceiptService receiptService)
        {
            _paymentService = paymentService;
            _receiptService = receiptService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var model = new AdminPaymentsIndexViewModel
            {
                Search = search,
                Payments = await _paymentService.GetAllPaymentsAsync(search),
                Invoices = await _paymentService.GetAllInvoicesAsync()
            };
            return View(model);
        }

        // One payment: who paid, for which learner(s), when, Stripe references.
        [HttpGet]
        public async Task<IActionResult> Payment(int paymentId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();
            return View(payment);
        }

        // Admin copy of the receipt (extra payment details section).
        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int paymentId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();

            return File(_receiptService.GeneratePaymentReceiptPdf(payment, adminView: true),
                "application/pdf", _receiptService.GetReceiptFileName(payment));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateDueRecurring()
        {
            var result = await _paymentService.GenerateDueRecurringInvoicesAsync();

            TempData["SuccessMessage"] =
                $"{result.Generated} invoice(s) generated ({result.CarriedForwardLines} unpaid balance(s) carried forward), {result.Skipped} not due yet.";
            if (result.Errors.Any())
                TempData["ErrorMessage"] = string.Join(" | ", result.Errors);

            return RedirectToAction(nameof(Index));
        }
    }
}