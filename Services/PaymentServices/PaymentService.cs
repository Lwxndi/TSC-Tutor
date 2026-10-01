using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Notifications;

namespace Tutor_Manager.Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly ILogger<PaymentService> _logger;
        private readonly INotificationService _notifications;

        // A line is "unpaid" while it is in one of these states.
        private static readonly LineItemStatus[] UnpaidStatuses =
            { LineItemStatus.Pending, LineItemStatus.PartiallyPaid, LineItemStatus.Overdue };

        public PaymentService(
            Tutor_ManagerDatabaseContext context,
            ILogger<PaymentService> logger,
            INotificationService notifications)
        {
            _context = context;
            _logger = logger;
            _notifications = notifications;
        }

        // ============================================================
        // INVOICE CREATION
        // ============================================================

        public async Task<PaymentResult> GenerateFirstInvoiceAsync(int enrollmentId, int guardianUserId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
                return PaymentResult.Failure("Enrollment not found.");

            if (enrollment.Status != EnrollmentStatus.AwaitingPayment)
                return PaymentResult.Failure("First invoices are only generated for enrollments that are awaiting payment.");

            if (!await _context.Parents.AnyAsync(p => p.UserId == guardianUserId))
                return PaymentResult.Failure("Guardian not found.");

            // Belt-and-braces against double billing (a retried request, a race). Voided lines
            // don't count — a redirect voids the old invoice and issues a fresh one.
            var alreadyBilled = await _context.InvoiceLineItems
                .AnyAsync(li => li.EnrollmentId == enrollmentId && li.Status != LineItemStatus.Voided);
            if (alreadyBilled)
            {
                _logger.LogWarning("GenerateFirstInvoiceAsync called for enrollment {EnrollmentId}, which already has a live line item.", enrollmentId);
                return PaymentResult.Failure("An invoice has already been generated for this enrollment.");
            }

            // Read the Offering by id (not via enrollment.Offering) so a redirect that just changed
            // OfferingId is always priced from the NEW offering.
            var offering = await _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .FirstAsync(o => o.OfferingId == enrollment.OfferingId);

            var invoice = new Invoice
            {
                GuardianUserId = guardianUserId,
                TotalAmountDue = offering.Price,
                TotalAmountPaidSoFar = 0,
                Status = InvoiceStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            invoice.LineItems.Add(new InvoiceLineItem
            {
                LearnerUserId = enrollment.LearnerUserId,
                EnrollmentId = enrollment.EnrollmentId,
                OfferingSnapshotId = offering.OfferingId,
                BillingType = offering.BillingType,
                AmountDue = offering.Price,
                AmountPaid = 0,
                Status = LineItemStatus.Pending,
                DueDate = DateTime.UtcNow.Date,
                Description = BuildLineItemDescription(offering)
            });

            _context.Invoices.Add(invoice);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "First invoice generation failed for enrollment {EnrollmentId}", enrollmentId);
                return PaymentResult.Failure("The invoice could not be saved. Please try again.");
            }

            return PaymentResult.Success(invoice.InvoiceId);
        }

        public async Task<PaymentResult> RegenerateFirstInvoiceAsync(int enrollmentId)
        {
            var guardianUserId = await _context.InvoiceLineItems
                .Where(li => li.EnrollmentId == enrollmentId && li.Invoice.Status == InvoiceStatus.Open)
                .Select(li => (int?)li.Invoice.GuardianUserId)
                .FirstOrDefaultAsync();

            if (guardianUserId == null)
                return PaymentResult.Failure("No open invoice was found to re-issue for this enrollment.");

            await VoidUnpaidInvoicesForEnrollmentAsync(enrollmentId, "Offering changed before payment; invoice re-issued.");
            return await GenerateFirstInvoiceAsync(enrollmentId, guardianUserId.Value);
        }

        public async Task<int> VoidUnpaidInvoicesForEnrollmentAsync(int enrollmentId, string reason)
        {
            // Find candidates, kill any in-flight Stripe sessions FIRST (this can settle a payment
            // that was just completed), then load fresh so we only void what is still Open.
            var candidateIds = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Open && i.LineItems.Any(li => li.EnrollmentId == enrollmentId))
                .Select(i => i.InvoiceId)
                .ToListAsync();

            if (candidateIds.Count == 0) return 0;

            await ExpirePendingPaymentsAsync(candidateIds);

            var invoices = await _context.Invoices
                .Include(i => i.LineItems)
                .Where(i => candidateIds.Contains(i.InvoiceId) && i.Status == InvoiceStatus.Open)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var invoice in invoices)
            {
                invoice.Status = InvoiceStatus.Voided;
                invoice.VoidedAt = now;
                invoice.VoidReason = reason.Length > 300 ? reason[..300] : reason;

                foreach (var li in invoice.LineItems.Where(IsUnpaid))
                    li.Status = LineItemStatus.Voided;
            }

            await _context.SaveChangesAsync();
            return invoices.Count;
        }

        public async Task<RecurringBillingResult> GenerateDueRecurringInvoicesAsync()
        {
            var result = new RecurringBillingResult();
            var today = DateTime.UtcNow.Date;

            // Active + Recurring only. AwaitingPayment / Suspended / Withdrawn never match, and
            // OnceOff offerings never get a second invoice.
            var candidates = await _context.Enrollments
                .Include(e => e.Offering).ThenInclude(o => o.Subject)
                .Include(e => e.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Where(e => e.Status == EnrollmentStatus.Active
                    && e.Offering.BillingType == BillingType.Recurring)
                .ToListAsync();

            foreach (var enrollment in candidates)
            {
                try
                {
                    // Billing history of the enrollment's own monthly lines (carried-forward and
                    // voided lines are not billing months).
                    var history = await _context.InvoiceLineItems
                        .Where(li => li.EnrollmentId == enrollment.EnrollmentId
                            && !li.IsCarriedForward
                            && li.Status != LineItemStatus.Voided)
                        .OrderByDescending(li => li.DueDate)
                        .Select(li => new { li.DueDate, li.Invoice.GuardianUserId })
                        .ToListAsync();

                    if (history.Count == 0)
                    {
                        result.Skipped++;
                        result.Errors.Add($"Enrollment {enrollment.EnrollmentId}: no prior invoice found, skipped.");
                        continue;
                    }

                    // Anchor to the FIRST due date so a 31st never drifts to the 28th.
                    var anchor = history[^1].DueDate;
                    var last = history[0].DueDate;
                    var monthsElapsed = (last.Year - anchor.Year) * 12 + (last.Month - anchor.Month);
                    var nextDueDate = anchor.AddMonths(monthsElapsed + 1);

                    if (today < nextDueDate)
                    {
                        result.Skipped++;
                        continue;
                    }

                    // Idempotency — running twice in the same cycle must not double-bill.
                    var alreadyBilledThisCycle = await _context.InvoiceLineItems.AnyAsync(li =>
                        li.EnrollmentId == enrollment.EnrollmentId
                        && !li.IsCarriedForward
                        && li.Status != LineItemStatus.Voided
                        && li.DueDate == nextDueDate);
                    if (alreadyBilledThisCycle)
                    {
                        result.Skipped++;
                        continue;
                    }

                    // Billing continuity follows whoever was billed last cycle.
                    var guardianUserId = history[0].GuardianUserId;

                    // Settle / expire any in-flight Stripe sessions on the invoices we're about to
                    // carry, so nothing can be paid twice.
                    var openInvoiceIds = await _context.InvoiceLineItems
                        .Where(li => li.EnrollmentId == enrollment.EnrollmentId
                            && li.Invoice.GuardianUserId == guardianUserId
                            && li.Invoice.Status == InvoiceStatus.Open
                            && UnpaidStatuses.Contains(li.Status))
                        .Select(li => li.InvoiceId)
                        .Distinct()
                        .ToListAsync();
                    await ExpirePendingPaymentsAsync(openInvoiceIds);

                    var unpaidLines = await _context.InvoiceLineItems
                        .Include(li => li.OfferingSnapshot).ThenInclude(o => o.Subject)
                        .Include(li => li.Invoice).ThenInclude(i => i.LineItems)
                        .Where(li => li.EnrollmentId == enrollment.EnrollmentId
                            && li.Invoice.GuardianUserId == guardianUserId
                            && li.Invoice.Status == InvoiceStatus.Open
                            && UnpaidStatuses.Contains(li.Status))
                        .OrderBy(li => li.DueDate)
                        .ToListAsync();

                    var offering = enrollment.Offering; // live price — redirect price applies from here on

                    var invoice = new Invoice
                    {
                        GuardianUserId = guardianUserId,
                        Status = InvoiceStatus.Open,
                        CreatedAt = DateTime.UtcNow
                    };

                    // This month's fee.
                    invoice.LineItems.Add(new InvoiceLineItem
                    {
                        LearnerUserId = enrollment.LearnerUserId,
                        EnrollmentId = enrollment.EnrollmentId,
                        OfferingSnapshotId = enrollment.OfferingId,
                        BillingType = offering.BillingType,
                        AmountDue = offering.Price,
                        AmountPaid = 0,
                        Status = LineItemStatus.Pending,
                        DueDate = nextDueDate,
                        Description = BuildLineItemDescription(offering)
                    });

                    // Unpaid earlier balances, carried in as their own labelled lines.
                    var total = offering.Price;
                    foreach (var old in unpaidLines)
                    {
                        var outstanding = old.AmountDue - old.AmountPaid;
                        invoice.LineItems.Add(new InvoiceLineItem
                        {
                            LearnerUserId = old.LearnerUserId,
                            EnrollmentId = old.EnrollmentId,
                            OfferingSnapshotId = old.OfferingSnapshotId,
                            BillingType = old.BillingType,
                            AmountDue = outstanding,
                            AmountPaid = 0,
                            Status = LineItemStatus.Pending,
                            DueDate = old.DueDate, // original month, so receipts show where the debt is from
                            IsCarriedForward = true,
                            CarriedFromLineItemId = old.InvoiceLineItemId,
                            Description = BuildCarriedDescription(old)
                        });
                        total += outstanding;

                        old.Status = LineItemStatus.CarriedForward;
                        result.CarriedForwardLines++;
                    }

                    invoice.TotalAmountDue = total;
                    invoice.TotalAmountPaidSoFar = 0;

                    // An old invoice whose lines are all settled/carried is itself carried forward.
                    foreach (var oldInvoice in unpaidLines.Select(l => l.Invoice).Distinct())
                    {
                        if (oldInvoice.LineItems.All(li => !IsUnpaid(li)))
                        {
                            oldInvoice.Status = InvoiceStatus.CarriedForward;
                            oldInvoice.CarriedForwardIntoInvoice = invoice;
                        }
                    }

                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync(); // one save: new invoice + old ones marked, atomically
                    result.Generated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Recurring invoice generation failed for enrollment {EnrollmentId}", enrollment.EnrollmentId);
                    result.Errors.Add($"Enrollment {enrollment.EnrollmentId}: {ex.Message}");
                }
            }

            return result;
        }

        // ============================================================
        // PAYING
        // ============================================================

        public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
            IReadOnlyCollection<int> invoiceIds, int guardianUserId, string successUrlTemplate, string cancelUrl)
        {
            var ids = (invoiceIds ?? Array.Empty<int>()).Distinct().ToList();
            if (ids.Count == 0)
                return CheckoutSessionResult.Failure("Select at least one invoice to pay.");

            // 1. Existence + ownership, before touching Stripe.
            var owners = await _context.Invoices.AsNoTracking()
                .Where(i => ids.Contains(i.InvoiceId))
                .Select(i => new { i.InvoiceId, i.GuardianUserId })
                .ToListAsync();
            if (owners.Count != ids.Count)
                return CheckoutSessionResult.Failure("One or more invoices were not found.");
            if (owners.Any(o => o.GuardianUserId != guardianUserId))
                return CheckoutSessionResult.Failure("One or more invoices do not belong to you.");

            // 2. Any earlier open session covering these invoices is settled or expired, so the
            //    guardian can never have two live sessions for the same invoice.
            await ExpirePendingPaymentsAsync(ids);

            // 3. Fresh read of what is actually payable now.
            var invoices = await _context.Invoices.AsNoTracking()
                .Include(i => i.Guardian).ThenInclude(g => g.User)
                .Include(i => i.LineItems).ThenInclude(li => li.Learner).ThenInclude(l => l.User)
                .Include(i => i.LineItems).ThenInclude(li => li.OfferingSnapshot).ThenInclude(o => o.Subject)
                .AsSplitQuery()
                .Where(i => ids.Contains(i.InvoiceId))
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                if (invoice.Status != InvoiceStatus.Open)
                    return CheckoutSessionResult.Failure(
                        $"Invoice INV-{invoice.InvoiceId:D6} can no longer be paid ({invoice.Status}). Please refresh the page.");
            }

            var payableLines = invoices.SelectMany(i => i.LineItems).Where(IsUnpaid).ToList();
            var total = payableLines.Sum(li => li.AmountDue - li.AmountPaid);
            if (payableLines.Count == 0 || total <= 0)
                return CheckoutSessionResult.Failure("There is nothing left to pay on the selected invoices.");

            // 4. Record the attempt BEFORE going to Stripe.
            var payment = new Payment
            {
                GuardianUserId = guardianUserId,
                Amount = total,
                Currency = "ZAR",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            foreach (var invoice in invoices)
            {
                payment.PaymentInvoices.Add(new PaymentInvoice
                {
                    InvoiceId = invoice.InvoiceId,
                    AmountApplied = invoice.LineItems.Where(IsUnpaid).Sum(li => li.AmountDue - li.AmountPaid)
                });
            }
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 5. Stripe Checkout Session.
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                Mode = "payment", // one-time; every month is its own invoice in our system
                SuccessUrl = successUrlTemplate,
                CancelUrl = cancelUrl,
                CustomerEmail = invoices[0].Guardian.User.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Metadata = new Dictionary<string, string> { ["PaymentId"] = payment.PaymentId.ToString() },
                LineItems = payableLines.Select(li => new Stripe.Checkout.SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        Currency = "zar",
                        UnitAmount = ToCents(li.AmountDue - li.AmountPaid),
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = Truncate($"{li.Learner.User.FirstName} {li.Learner.User.LastName} — {DescribeLine(li)}", 250)
                        }
                    }
                }).ToList()
            };

            try
            {
                var session = await new Stripe.Checkout.SessionService().CreateAsync(options);
                payment.StripeCheckoutSessionId = session.Id;
                await _context.SaveChangesAsync();
                return CheckoutSessionResult.Success(session.Url, payment.PaymentId);
            }
            catch (Stripe.StripeException ex)
            {
                _logger.LogError(ex, "Stripe Checkout Session creation failed for payment {PaymentId}", payment.PaymentId);
                payment.Status = PaymentStatus.Failed;
                await _context.SaveChangesAsync();
                return CheckoutSessionResult.Failure("Could not start the payment process. Please try again.");
            }
        }

        public async Task<PaymentVerificationResult> VerifyCheckoutSessionAsync(string sessionId, int guardianUserId)
        {
            var payment = await _context.Payments.AsNoTracking()
                .Where(p => p.StripeCheckoutSessionId == sessionId)
                .Select(p => new { p.PaymentId, p.GuardianUserId, p.Status })
                .FirstOrDefaultAsync();

            if (payment == null || payment.GuardianUserId != guardianUserId)
                return new PaymentVerificationResult { Found = false };

            if (payment.Status == PaymentStatus.Paid)
                return new PaymentVerificationResult { Found = true, PaymentId = payment.PaymentId, Status = PaymentStatus.Paid };

            try
            {
                var session = await new Stripe.Checkout.SessionService().GetAsync(sessionId);
                if (session.PaymentStatus == "paid")
                {
                    var applied = await ApplyPaymentAsync(payment.PaymentId, session.PaymentIntentId, session.AmountTotal, session.Currency);
                    if (!applied.Succeeded)
                        _logger.LogError("Success page could not apply payment {PaymentId}: {Error}", payment.PaymentId, applied.ErrorMessage);
                }
            }
            catch (Stripe.StripeException ex)
            {
                _logger.LogError(ex, "Could not verify Stripe session {SessionId} on the success page", sessionId);
            }

            var status = await _context.Payments.AsNoTracking()
                .Where(p => p.PaymentId == payment.PaymentId)
                .Select(p => p.Status)
                .FirstAsync();

            return new PaymentVerificationResult { Found = true, PaymentId = payment.PaymentId, Status = status };
        }

        // ============================================================
        // WEBHOOK ENTRY POINTS
        // ============================================================

        public async Task<PaymentApplyResult> ProcessPaidSessionAsync(Stripe.Checkout.Session session)
        {
            var paymentId = await ResolvePaymentIdAsync(session);
            if (paymentId == null)
                return PaymentApplyResult.Failure($"No payment record found for Stripe session {session.Id}.");

            return await ApplyPaymentAsync(paymentId.Value, session.PaymentIntentId, session.AmountTotal, session.Currency);
        }

        public async Task HandleSessionExpiredAsync(Stripe.Checkout.Session session)
        {
            var paymentId = await ResolvePaymentIdAsync(session);
            if (paymentId == null) return;

            // Only a still-Pending payment can expire. Invoices were never locked, so they are
            // simply payable again.
            await _context.Payments
                .Where(p => p.PaymentId == paymentId && p.Status == PaymentStatus.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PaymentStatus.Expired));
        }

        public async Task HandleSessionFailedAsync(Stripe.Checkout.Session session)
        {
            var paymentId = await ResolvePaymentIdAsync(session);
            if (paymentId == null) return;

            var changed = await _context.Payments
                .Where(p => p.PaymentId == paymentId && p.Status == PaymentStatus.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PaymentStatus.Failed));
            if (changed == 0) return;

            try
            {
                var info = await _context.Payments.AsNoTracking()
                    .Where(p => p.PaymentId == paymentId)
                    .Select(p => new { p.GuardianUserId, p.Amount })
                    .FirstAsync();
                await _notifications.SendAsync(info.GuardianUserId, NotificationType.PaymentFailed,
                    new Dictionary<string, string> { ["Amount"] = info.Amount.ToString("N2", CultureInfo.InvariantCulture) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send payment-failed notification for payment {PaymentId}", paymentId);
            }
        }

        // ------------------------------------------------------------
        // The single place a payment is confirmed. Both the webhook and the Success page end up
        // here. Concurrency safety comes from the claim below: a conditional UPDATE that only one
        // caller can win; the loser sees 0 rows and does nothing. Payment + invoices + enrollment
        // activation commit together or not at all.
        // ------------------------------------------------------------
        private async Task<PaymentApplyResult> ApplyPaymentAsync(
            int paymentId, string? paymentIntentId, long? amountTotalCents, string? currency)
        {
            var snapshot = await _context.Payments.AsNoTracking()
                .Where(p => p.PaymentId == paymentId)
                .Select(p => new { p.Status, p.Amount, p.Currency })
                .FirstOrDefaultAsync();

            if (snapshot == null)
                return PaymentApplyResult.Failure($"Payment {paymentId} not found.", paymentId);

            if (snapshot.Status == PaymentStatus.Paid)
                return PaymentApplyResult.Already(paymentId);

            // What Stripe says it took must equal what we asked for.
            var expectedCents = ToCents(snapshot.Amount);
            if (amountTotalCents != expectedCents
                || !string.Equals(currency, snapshot.Currency, StringComparison.OrdinalIgnoreCase))
            {
                var note = $"Stripe reported {amountTotalCents} {currency} but the payment was created for {expectedCents} {snapshot.Currency}. Nothing was applied; review in Stripe.";
                _logger.LogError("Payment {PaymentId} mismatch: {Note}", paymentId, note);
                await _context.Payments
                    .Where(p => p.PaymentId == paymentId && p.Status != PaymentStatus.Paid)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Status, PaymentStatus.NeedsReview)
                        .SetProperty(p => p.ReviewNote, note)
                        .SetProperty(p => p.StripePaymentIntentId, paymentIntentId));
                return PaymentApplyResult.Failure(note, paymentId);
            }

            // Reuse an ambient transaction if a caller already opened one on this context.
            var ownsTransaction = _context.Database.CurrentTransaction == null;
            var tx = ownsTransaction ? await _context.Database.BeginTransactionAsync() : null;

            Payment payment;
            var learnerNames = new List<string>();
            try
            {
                var now = DateTime.UtcNow;

                // CLAIM — only one caller can flip a not-yet-Paid payment to Paid.
                var claimed = await _context.Payments
                    .Where(p => p.PaymentId == paymentId && p.Status != PaymentStatus.Paid)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Status, PaymentStatus.Paid)
                        .SetProperty(p => p.PaidAt, now)
                        .SetProperty(p => p.StripePaymentIntentId, paymentIntentId));

                if (claimed == 0)
                {
                    if (tx != null) await tx.RollbackAsync();
                    return PaymentApplyResult.Already(paymentId);
                }

                payment = await _context.Payments
                    .Include(p => p.Guardian).ThenInclude(g => g.User)
                    .Include(p => p.PaymentInvoices).ThenInclude(pi => pi.Invoice).ThenInclude(i => i.LineItems).ThenInclude(li => li.Enrollment)
                    .Include(p => p.PaymentInvoices).ThenInclude(pi => pi.Invoice).ThenInclude(i => i.LineItems).ThenInclude(li => li.Learner).ThenInclude(l => l.User)
                    .AsSplitQuery()
                    .FirstAsync(p => p.PaymentId == paymentId);

                var anomalies = new List<string>();

                foreach (var pi in payment.PaymentInvoices)
                {
                    var invoice = pi.Invoice;

                    if (invoice.Status != InvoiceStatus.Open)
                    {
                        anomalies.Add($"INV-{invoice.InvoiceId:D6} was {invoice.Status} when R{pi.AmountApplied:N2} arrived — refund or reallocate manually.");
                        continue;
                    }

                    foreach (var li in invoice.LineItems.Where(IsUnpaid))
                    {
                        li.AmountPaid = li.AmountDue;
                        li.Status = LineItemStatus.Paid;
                        learnerNames.Add($"{li.Learner.User.FirstName} {li.Learner.User.LastName}");

                        // Payment is what grants access: first payment activates, and a payment
                        // on a suspended enrollment restores it. (Done here, not via
                        // IEnrollmentService, to keep it in this transaction and avoid the
                        // PaymentService <-> EnrollmentService circular dependency.)
                        var enrollment = li.Enrollment;
                        switch (enrollment.Status)
                        {
                            case EnrollmentStatus.AwaitingPayment:
                                enrollment.Status = EnrollmentStatus.Active;
                                break;
                            case EnrollmentStatus.Suspended:
                                enrollment.Status = EnrollmentStatus.Active;
                                enrollment.SuspendedAt = null;
                                break;
                            case EnrollmentStatus.Rejected:
                                anomalies.Add($"Enrollment {enrollment.EnrollmentId} is Rejected but was paid for — refund manually.");
                                break;
                        }
                    }

                    invoice.TotalAmountPaidSoFar = invoice.TotalAmountDue;
                    invoice.Status = InvoiceStatus.Paid;
                    invoice.PaidAt = now;
                    invoice.StripePaymentIntentId = paymentIntentId;
                }

                if (anomalies.Count > 0)
                {
                    payment.ReviewNote = Truncate(string.Join(" | ", anomalies), 1000);
                    _logger.LogError("Payment {PaymentId} applied with anomalies: {Note}", paymentId, payment.ReviewNote);
                }

                await _context.SaveChangesAsync();
                if (tx != null) await tx.CommitAsync();
            }
            catch
            {
                if (tx != null) await tx.RollbackAsync(); // payment stays Pending -> Stripe retries the webhook
                throw;
            }
            finally
            {
                if (tx != null) await tx.DisposeAsync();
            }

            // Side effects run only for the caller that won the claim, so they happen once.
            await SendPaymentNotificationsAsync(payment, learnerNames);

            return PaymentApplyResult.Applied(paymentId);
        }

        private async Task SendPaymentNotificationsAsync(Payment payment, List<string> learnerNames)
        {
            try
            {
                var amount = payment.Amount.ToString("N2", CultureInfo.InvariantCulture);
                var guardian = payment.Guardian.User;

                await _notifications.SendAsync(payment.GuardianUserId, NotificationType.PaymentReceived,
                    new Dictionary<string, string> { ["Amount"] = amount });

                await _notifications.NotifyAdminsAsync(NotificationType.PaymentReceivedAdmin,
                    new Dictionary<string, string>
                    {
                        ["GuardianName"] = $"{guardian.FirstName} {guardian.LastName}",
                        ["Amount"] = amount,
                        ["Learners"] = learnerNames.Count > 0 ? string.Join(", ", learnerNames.Distinct()) : "—"
                    });
            }
            catch (Exception ex)
            {
                // The money is safely recorded; a failed notification must never undo or fail that.
                _logger.LogError(ex, "Payment {PaymentId} was applied but notifications failed", payment.PaymentId);
            }
        }

        // For each still-Pending payment covering any of these invoices: if Stripe says it was
        // actually paid, settle it; otherwise expire the Stripe session and mark it Expired.
        private async Task ExpirePendingPaymentsAsync(List<int> invoiceIds)
        {
            if (invoiceIds.Count == 0) return;

            var pending = await _context.Payments.AsNoTracking()
                .Where(p => p.Status == PaymentStatus.Pending
                    && p.PaymentInvoices.Any(pi => invoiceIds.Contains(pi.InvoiceId)))
                .Select(p => new { p.PaymentId, p.StripeCheckoutSessionId })
                .ToListAsync();

            if (pending.Count == 0) return;

            var sessions = new Stripe.Checkout.SessionService();
            foreach (var p in pending)
            {
                if (!string.IsNullOrEmpty(p.StripeCheckoutSessionId))
                {
                    try
                    {
                        var session = await sessions.GetAsync(p.StripeCheckoutSessionId);

                        if (session.PaymentStatus == "paid")
                        {
                            await ApplyPaymentAsync(p.PaymentId, session.PaymentIntentId, session.AmountTotal, session.Currency);
                            continue;
                        }

                        if (session.Status == "open")
                            await sessions.ExpireAsync(p.StripeCheckoutSessionId);
                    }
                    catch (Stripe.StripeException ex)
                    {
                        _logger.LogWarning(ex, "Could not expire Stripe session {SessionId}", p.StripeCheckoutSessionId);
                    }
                }

                await _context.Payments
                    .Where(x => x.PaymentId == p.PaymentId && x.Status == PaymentStatus.Pending)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, PaymentStatus.Expired));
            }
        }

        private async Task<int?> ResolvePaymentIdAsync(Stripe.Checkout.Session session)
        {
            if (session.Metadata != null
                && session.Metadata.TryGetValue("PaymentId", out var raw)
                && int.TryParse(raw, out var id))
                return id;

            return await _context.Payments.AsNoTracking()
                .Where(p => p.StripeCheckoutSessionId == session.Id)
                .Select(p => (int?)p.PaymentId)
                .FirstOrDefaultAsync();
        }

        // ============================================================
        // READS
        // ============================================================

        public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId) =>
            await _context.Invoices.AsNoTracking()
                .Include(i => i.Guardian).ThenInclude(g => g.User)
                .Include(i => i.LineItems).ThenInclude(li => li.Learner).ThenInclude(l => l.User)
                .Include(i => i.LineItems).ThenInclude(li => li.OfferingSnapshot).ThenInclude(o => o.Subject)
                .AsSplitQuery()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        public async Task<List<Invoice>> GetInvoicesForGuardianAsync(int guardianUserId) =>
            await _context.Invoices.AsNoTracking()
                .Where(i => i.GuardianUserId == guardianUserId)
                .Include(i => i.LineItems).ThenInclude(li => li.Learner).ThenInclude(l => l.User)
                .Include(i => i.LineItems).ThenInclude(li => li.OfferingSnapshot).ThenInclude(o => o.Subject)
                .OrderByDescending(i => i.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();

        public async Task<List<Invoice>> GetPayableInvoicesForGuardianAsync(int guardianUserId) =>
            await _context.Invoices.AsNoTracking()
                .Where(i => i.GuardianUserId == guardianUserId && i.Status == InvoiceStatus.Open)
                .Include(i => i.LineItems).ThenInclude(li => li.Learner).ThenInclude(l => l.User)
                .Include(i => i.LineItems).ThenInclude(li => li.OfferingSnapshot).ThenInclude(o => o.Subject)
                .OrderBy(i => i.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();

        public async Task<List<Invoice>> GetAllInvoicesAsync() =>
            await _context.Invoices.AsNoTracking()
                .Include(i => i.Guardian).ThenInclude(g => g.User)
                .Include(i => i.LineItems).ThenInclude(li => li.Learner).ThenInclude(l => l.User)
                .Include(i => i.LineItems).ThenInclude(li => li.OfferingSnapshot).ThenInclude(o => o.Subject)
                .OrderByDescending(i => i.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId) =>
            await PaymentsWithDetails().FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        public async Task<List<Payment>> GetPaidPaymentsForGuardianAsync(int guardianUserId) =>
            await PaymentsWithDetails()
                .Where(p => p.GuardianUserId == guardianUserId && p.Status == PaymentStatus.Paid)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();

        public async Task<List<Payment>> GetAllPaymentsAsync(string? search = null)
        {
            var query = PaymentsWithDetails();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(p =>
                    p.PaymentId.ToString() == s
                    || p.Guardian.User.FirstName.Contains(s)
                    || p.Guardian.User.LastName.Contains(s)
                    || p.Guardian.User.Email.Contains(s)
                    || (p.StripePaymentIntentId != null && p.StripePaymentIntentId.Contains(s))
                    || p.PaymentInvoices.Any(pi => pi.Invoice.LineItems.Any(li =>
                        li.Learner.User.FirstName.Contains(s) || li.Learner.User.LastName.Contains(s))));
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        // Everything a receipt needs: guardian, and for every covered invoice its lines with learner + subject.
        private IQueryable<Payment> PaymentsWithDetails() =>
            _context.Payments.AsNoTracking()
                .Include(p => p.Guardian).ThenInclude(g => g.User)
                .Include(p => p.PaymentInvoices).ThenInclude(pi => pi.Invoice).ThenInclude(i => i.LineItems).ThenInclude(li => li.Learner).ThenInclude(l => l.User)
                .Include(p => p.PaymentInvoices).ThenInclude(pi => pi.Invoice).ThenInclude(i => i.LineItems).ThenInclude(li => li.OfferingSnapshot).ThenInclude(o => o.Subject)
                .AsSplitQuery();

        // ============================================================
        // HELPERS
        // ============================================================

        private static bool IsUnpaid(InvoiceLineItem li) =>
            li.Status is LineItemStatus.Pending or LineItemStatus.PartiallyPaid or LineItemStatus.Overdue;

        private static long ToCents(decimal amount) =>
            (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

        private static string DescribeLine(InvoiceLineItem li) =>
            li.Description ?? $"{li.OfferingSnapshot.Subject.SubjectName} — Grade {(int)li.OfferingSnapshot.Grade}";

        // Requires offering.Subject and offering.Tutor.User to be loaded by the caller.
        private static string BuildLineItemDescription(Offering offering) =>
            Truncate($"{offering.Subject.SubjectName} — Grade {(int)offering.Grade} — " +
                     $"{offering.Tutor.User.FirstName} {offering.Tutor.User.LastName} ({offering.DeliveryMethod})", 300);

        // "Outstanding from Oct 2026 — Mathematics — Grade 11 — ...". Already-carried lines keep
        // their original label rather than nesting a second prefix.
        private static string BuildCarriedDescription(InvoiceLineItem old)
        {
            if (old.IsCarriedForward && old.Description != null)
                return old.Description;

            var month = old.DueDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);
            return Truncate($"Outstanding from {month} — {DescribeLine(old)}", 300);
        }
    }
}