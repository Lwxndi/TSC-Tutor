using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Tutor_Manager.Options;
using Tutor_Manager.Services.PaymentServices;

namespace Tutor_Manager.Controllers
{
    // Stripe calls this directly — no logged-in user, so [AllowAnonymous] is required. The
    // Stripe-Signature header (verified against WebhookSecret) is what proves the caller is Stripe.
    //
    // Response rules:
    //   400 -> signature failed (never process).
    //   200 -> handled, OR permanently unhandleable (retrying the same event can't fix it).
    //   500 -> unexpected/transient error; Stripe retries, which is safe because applying a
    //          payment is idempotent.
    //
    // Register these events in the Stripe dashboard / CLI:
    //   checkout.session.completed, checkout.session.async_payment_succeeded,
    //   checkout.session.async_payment_failed, checkout.session.expired
    [ApiController]
    [Route("webhook/stripe")]
    [AllowAnonymous]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly StripeOptions _options;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IPaymentService paymentService,
            IOptions<StripeOptions> options,
            ILogger<StripeWebhookController> logger)
        {
            _paymentService = paymentService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _options.WebhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return BadRequest();
            }

            try
            {
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                    case "checkout.session.async_payment_succeeded":
                        {
                            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                            if (session == null)
                            {
                                _logger.LogError("Stripe {Type} event had no checkout session payload.", stripeEvent.Type);
                                return Ok();
                            }

                            // "completed" fires when the customer finishes checkout, which for
                            // delayed payment methods is BEFORE the money arrives. Only "paid" counts.
                            if (session.PaymentStatus != "paid")
                            {
                                _logger.LogInformation("Session {SessionId} completed but payment_status is {Status}; waiting for async result.", session.Id, session.PaymentStatus);
                                return Ok();
                            }

                            var result = await _paymentService.ProcessPaidSessionAsync(session);
                            if (!result.Succeeded)
                                _logger.LogError("Could not apply payment for session {SessionId}: {Error}", session.Id, result.ErrorMessage);

                            return Ok();
                        }

                    case "checkout.session.async_payment_failed":
                        {
                            if (stripeEvent.Data.Object is Stripe.Checkout.Session session)
                                await _paymentService.HandleSessionFailedAsync(session);
                            return Ok();
                        }

                    case "checkout.session.expired":
                        {
                            if (stripeEvent.Data.Object is Stripe.Checkout.Session session)
                                await _paymentService.HandleSessionExpiredAsync(session);
                            return Ok();
                        }

                    default:
                        return Ok(); // not an event we act on
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling Stripe event {EventId} ({Type})", stripeEvent.Id, stripeEvent.Type);
                return StatusCode(500);
            }
        }
    }
}