using System.Globalization;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Options;

namespace Tutor_Manager.Services.ReceiptServices
{
    public class ReceiptService : IReceiptService
    {
        private readonly BusinessOptions _business;

        public ReceiptService(IOptions<BusinessOptions> business)
        {
            _business = business.Value;
        }

        private record ReceiptRow(string Learner, string Description, string Month, string InvoiceRef, decimal Amount);

        public string GetInvoiceReference(Invoice invoice) => $"INV-{invoice.InvoiceId:D6}";
        public string GetReceiptReference(Payment payment) => $"RCT-{payment.PaymentId:D6}";
        public string GetInvoiceFileName(Invoice invoice) => $"Invoice-{GetInvoiceReference(invoice)}.pdf";
        public string GetReceiptFileName(Payment payment) => $"Receipt-{GetReceiptReference(payment)}.pdf";

        // ============================================================
        // INVOICE PDF
        // ============================================================
        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            var reference = GetInvoiceReference(invoice);
            var guardian = invoice.Guardian.User;
            var rows = BuildRows(invoice);
            var learners = DistinctLearners(rows);
            var balance = invoice.Status == InvoiceStatus.Open ? invoice.TotalAmountDue - invoice.TotalAmountPaidSoFar : 0m;

            var (statusText, statusColor) = invoice.Status switch
            {
                InvoiceStatus.Paid => ("PAID", Colors.Green.Darken2),
                InvoiceStatus.CarriedForward => ("CARRIED FORWARD — balance moved to a newer invoice", Colors.Blue.Darken2),
                InvoiceStatus.Voided => ("VOID", Colors.Red.Darken2),
                _ => ("OUTSTANDING", Colors.Orange.Darken2)
            };

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    StylePage(page);
                    ComposeHeader(page, "INVOICE", reference, $"Issued: {FormatDate(invoice.CreatedAt.ToLocalTime())}");

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(14);

                        col.Item().Column(c =>
                        {
                            c.Item().Text("Billed to").SemiBold();
                            c.Item().Text($"{guardian.FirstName} {guardian.LastName}");
                            c.Item().Text(guardian.Email);
                        });

                        col.Item().Text(t =>
                        {
                            t.Span("Learner(s): ").SemiBold();
                            t.Span(learners);
                        });

                        col.Item().Text(t =>
                        {
                            t.Span("Status: ").SemiBold();
                            t.Span(statusText).Bold().FontColor(statusColor);
                            if (invoice.Status == InvoiceStatus.Paid && invoice.PaidAt.HasValue)
                                t.Span($" on {FormatDateTime(invoice.PaidAt.Value.ToLocalTime())}");
                        });

                        AddLinesTable(col, rows);

                        col.Item().AlignRight().Width(220).Column(t =>
                        {
                            AddTotalRow(t, "Total", invoice.TotalAmountDue, false);
                            AddTotalRow(t, "Paid", invoice.TotalAmountPaidSoFar, false);
                            t.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(4)
                                .Element(e => AddTotalRow(e, "Balance", balance, true));
                        });
                    });

                    ComposeFooter(page, reference);
                });
            }).GeneratePdf();
        }

        // ============================================================
        // PAYMENT RECEIPT PDF (guardian + admin)
        // ============================================================
        public byte[] GeneratePaymentReceiptPdf(Payment payment, bool adminView)
        {
            var reference = GetReceiptReference(payment);
            var guardian = payment.Guardian.User;
            var invoices = payment.PaymentInvoices.Select(pi => pi.Invoice).OrderBy(i => i.InvoiceId).ToList();

            // Only lines this payment actually settled.
            var rows = invoices.SelectMany(i => BuildRows(i, onlyPaid: true)).ToList();
            var learners = DistinctLearners(rows);
            var paidAt = payment.PaidAt?.ToLocalTime();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    StylePage(page);
                    ComposeHeader(page,
                        adminView ? "PAYMENT RECEIPT (ADMIN COPY)" : "PAYMENT RECEIPT",
                        reference,
                        paidAt.HasValue ? $"Paid: {FormatDateTime(paidAt.Value)}" : "Payment date not recorded");

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(14);

                        col.Item().Column(c =>
                        {
                            c.Item().Text(adminView ? "Paid by (guardian)" : "Billed to").SemiBold();
                            c.Item().Text($"{guardian.FirstName} {guardian.LastName}");
                            c.Item().Text(guardian.Email);
                            if (adminView && !string.IsNullOrWhiteSpace(guardian.PhoneNumber))
                                c.Item().Text(guardian.PhoneNumber);
                        });

                        col.Item().Text(t =>
                        {
                            t.Span("Learner(s): ").SemiBold();
                            t.Span(learners);
                        });

                        col.Item().Text(t =>
                        {
                            t.Span("Status: ").SemiBold();
                            t.Span("PAID").Bold().FontColor(Colors.Green.Darken2);
                            if (paidAt.HasValue) t.Span($" on {FormatDateTime(paidAt.Value)}");
                        });

                        AddLinesTable(col, rows);

                        col.Item().AlignRight().Width(220).Column(t =>
                        {
                            t.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(4)
                                .Element(e => AddTotalRow(e, "Total paid", payment.Amount, true));
                        });

                        if (!string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
                        {
                            col.Item().Text(t =>
                            {
                                t.Span("Payment reference: ").SemiBold();
                                t.Span(payment.StripePaymentIntentId);
                            });
                        }

                        if (adminView)
                            ComposeAdminDetails(col, payment, rows, invoices);
                    });

                    ComposeFooter(page, reference);
                });
            }).GeneratePdf();
        }

        // Extra section only the admin copy carries.
        private void ComposeAdminDetails(ColumnDescriptor col, Payment payment, List<ReceiptRow> rows, List<Invoice> invoices)
        {
            col.Item().PaddingTop(6).Column(c =>
            {
                c.Spacing(3);
                c.Item().Text("Payment details (admin)").SemiBold().FontSize(11);

                c.Item().Text($"Payment ID: {payment.PaymentId}   |   Guardian user ID: {payment.GuardianUserId}");
                c.Item().Text($"Status: {payment.Status}   |   Currency: {payment.Currency}");
                c.Item().Text($"Started: {FormatDateTime(payment.CreatedAt.ToLocalTime())} (local)   |   " +
                              $"Paid: {(payment.PaidAt.HasValue ? payment.PaidAt.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC" : "—")}");
                c.Item().Text($"Invoices covered: {string.Join(", ", invoices.Select(GetInvoiceReference))}");
                c.Item().Text($"Stripe checkout session: {payment.StripeCheckoutSessionId ?? "—"}");
                c.Item().Text($"Stripe payment intent: {payment.StripePaymentIntentId ?? "—"}");

                if (!string.IsNullOrWhiteSpace(payment.ReviewNote))
                    c.Item().Text($"REVIEW NOTE: {payment.ReviewNote}").Bold().FontColor(Colors.Red.Darken2);

                c.Item().PaddingTop(6).Text("Paid per learner").SemiBold();
                foreach (var g in rows.GroupBy(r => r.Learner).OrderBy(g => g.Key))
                    c.Item().Text($"{g.Key}: {Money(g.Sum(r => r.Amount))}");
            });
        }

        // ============================================================
        // SHARED PIECES
        // ============================================================
        private static List<ReceiptRow> BuildRows(Invoice invoice, bool onlyPaid = false)
        {
            var ref_ = $"INV-{invoice.InvoiceId:D6}";
            return invoice.LineItems
                .Where(li => !onlyPaid || li.Status == LineItemStatus.Paid)
                .OrderBy(li => li.Learner.User.FirstName).ThenBy(li => li.DueDate)
                .Select(li => new ReceiptRow(
                    $"{li.Learner.User.FirstName} {li.Learner.User.LastName}",
                    li.Description ?? $"{li.OfferingSnapshot.Subject.SubjectName} — Grade {(int)li.OfferingSnapshot.Grade}",
                    li.DueDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ref_,
                    li.AmountDue))
                .ToList();
        }

        private static string DistinctLearners(IEnumerable<ReceiptRow> rows) =>
            string.Join(", ", rows.Select(r => r.Learner).Distinct());

        private static void AddLinesTable(ColumnDescriptor col, List<ReceiptRow> rows)
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2f);   // learner
                    c.RelativeColumn(4.2f); // description
                    c.RelativeColumn(1.3f); // billing month
                    c.RelativeColumn(1.5f); // invoice
                    c.RelativeColumn(1.5f); // amount
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Learner");
                    h.Cell().Element(HeaderCell).Text("Description");
                    h.Cell().Element(HeaderCell).Text("Month");
                    h.Cell().Element(HeaderCell).Text("Invoice");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Amount");
                });

                foreach (var r in rows)
                {
                    table.Cell().Element(BodyCell).Text(r.Learner);
                    table.Cell().Element(BodyCell).Text(r.Description);
                    table.Cell().Element(BodyCell).Text(r.Month);
                    table.Cell().Element(BodyCell).Text(r.InvoiceRef);
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(r.Amount));
                }
            });
        }

        private static void AddTotalRow(ColumnDescriptor t, string label, decimal amount, bool bold) =>
            t.Item().Element(e => AddTotalRow(e, label, amount, bold));

        private static void AddTotalRow(IContainer e, string label, decimal amount, bool bold)
        {
            e.Row(r =>
            {
                var l = r.RelativeItem().Text(label);
                var a = r.ConstantItem(90).AlignRight().Text(Money(amount));
                if (bold) { l.Bold(); a.Bold(); }
            });
        }

        private void ComposeHeader(PageDescriptor page, string title, string reference, string dateLine)
        {
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(_business.Name).FontSize(18).Bold();
                    if (!string.IsNullOrWhiteSpace(_business.Address)) col.Item().Text(_business.Address);
                    if (!string.IsNullOrWhiteSpace(_business.Email)) col.Item().Text(_business.Email);
                    if (!string.IsNullOrWhiteSpace(_business.Phone)) col.Item().Text(_business.Phone);
                });

                row.ConstantItem(200).AlignRight().Column(col =>
                {
                    col.Item().Text(title).FontSize(14).Bold();
                    col.Item().Text(reference);
                    col.Item().Text(dateLine);
                });
            });
        }

        private void ComposeFooter(PageDescriptor page, string reference)
        {
            page.Footer().AlignCenter().Text(t =>
            {
                t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                t.Span($"{_business.Name} · {reference} · Page ");
                t.CurrentPageNumber();
                t.Span(" of ");
                t.TotalPages();
            });
        }

        private static void StylePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));
        }

        private static IContainer HeaderCell(IContainer c) =>
            c.Background(Colors.Grey.Lighten3).Padding(5).DefaultTextStyle(x => x.SemiBold());

        private static IContainer BodyCell(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

        // Invariant culture keeps output identical regardless of server regional settings.
        private static string Money(decimal amount) =>
            "R" + amount.ToString("N2", CultureInfo.InvariantCulture);

        private static string FormatDate(DateTime d) =>
            d.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

        private static string FormatDateTime(DateTime d) =>
            d.ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture);
    }
}