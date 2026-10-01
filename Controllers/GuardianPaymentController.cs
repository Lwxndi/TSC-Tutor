using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutor_Manager.Services.PaymentServices;
using Tutor_Manager.Services.ReceiptServices;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Controllers
{
    [Authorize(Roles = "Parent")]
    public class GuardianPaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IReceiptService _receiptService;

        public GuardianPaymentController(IPaymentService paymentService, IReceiptService receiptService)
        {
            _paymentService = paymentService;
            _receiptService = receiptService;
        }

        private int GuardianUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Payments page: what can be paid now + the transactions table.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new GuardianPaymentsIndexViewModel
            {
                PayableInvoices = await _paymentService.GetPayableInvoicesForGuardianAsync(GuardianUserId),
                Transactions = await _paymentService.GetPaidPaymentsForGuardianAsync(GuardianUserId)
            };
            return View(model);
        }

        // One invoice. NotFound (not Forbid) for someone else's invoice — hides its existence.
        [HttpGet]
        public async Task<IActionResult> Details(int invoiceId)
        {
            var invoice = await _paymentService.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null || invoice.GuardianUserId != GuardianUserId)
                return NotFound();

            return View(invoice);
        }

        // Invoice PDF (for something not yet paid, or to re-read a past invoice).
        [HttpGet]
        public async Task<IActionResult> DownloadInvoice(int invoiceId)
        {
            var invoice = await _paymentService.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null || invoice.GuardianUserId != GuardianUserId)
                return NotFound();

            return File(_receiptService.GenerateInvoicePdf(invoice), "application/pdf", _receiptService.GetInvoiceFileName(invoice));
        }

        // One past transaction.
        [HttpGet]
        public async Task<IActionResult> Transaction(int paymentId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null || payment.GuardianUserId != GuardianUserId)
                return NotFound();

            return View(payment);
        }

        // Receipt PDF — same file from the Success page and from the transactions table.
        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int paymentId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null || payment.GuardianUserId != GuardianUserId)
                return NotFound();

            if (payment.Status != Tutor_Manager.Models.Enums.PaymentStatus.Paid)
                return NotFound(); // no receipt for a payment that hasn't happened

            return File(_receiptService.GeneratePaymentReceiptPdf(payment, adminView: false),
                "application/pdf", _receiptService.GetReceiptFileName(payment));
        }

        // Pay one or several invoices in a single Stripe payment. The form posts invoiceIds
        // (one checkbox per invoice).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int[] invoiceIds)
        {
            if (invoiceIds == null || invoiceIds.Length == 0)
            {
                TempData["ErrorMessage"] = "Select at least one invoice to pay.";
                return RedirectToAction(nameof(Index));
            }

            // Stripe replaces {CHECKOUT_SESSION_ID} with the real session id on redirect. The
            // placeholder swap avoids Url.Action URL-encoding the braces.
            const string placeholder = "SESSION_ID_PLACEHOLDER";
            var successUrl = Url.Action(nameof(Success), "GuardianPayment", new { sessionId = placeholder }, Request.Scheme)!
                .Replace(placeholder, "{CHECKOUT_SESSION_ID}");
            var cancelUrl = Url.Action(nameof(Cancelled), "GuardianPayment", null, Request.Scheme)!;

            var result = await _paymentService.CreateCheckoutSessionAsync(invoiceIds, GuardianUserId, successUrl, cancelUrl);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return Redirect(result.CheckoutUrl!);
        }

        // Stripe redirects here after payment. The page asks Stripe directly (so it never
        // depends on webhook timing) and applies the payment if the webhook hasn't yet.
        // Whichever of the two arrives first wins; the other is a safe no-op.
        [HttpGet]
        public async Task<IActionResult> Success(string? sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return RedirectToAction(nameof(Index));

            var verification = await _paymentService.VerifyCheckoutSessionAsync(sessionId, GuardianUserId);
            if (!verification.Found)
                return NotFound();

            var payment = await _paymentService.GetPaymentByIdAsync(verification.PaymentId!.Value);
            if (payment == null)
                return NotFound();

            return View(new PaymentSuccessViewModel { Payment = payment, Status = verification.Status });
        }

        [HttpGet]
        public IActionResult Cancelled() => View();
    }
}