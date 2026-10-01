using System.Globalization;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    // Small presentation helpers shared by the payment views, so the Razor stays readable and
    // references/amounts are formatted identically everywhere (and match the PDFs).
    public static class PaymentViewHelpers
    {
        public static string InvoiceRef(Invoice i) => $"INV-{i.InvoiceId:D6}";
        public static string ReceiptRef(Payment p) => $"RCT-{p.PaymentId:D6}";

        public static bool IsUnpaid(InvoiceLineItem li) =>
            li.Status is LineItemStatus.Pending or LineItemStatus.PartiallyPaid or LineItemStatus.Overdue;

        // What is still owed on an invoice.
        public static decimal Balance(Invoice i) =>
            i.LineItems.Where(IsUnpaid).Sum(li => li.AmountDue - li.AmountPaid);

        public static string Describe(InvoiceLineItem li) =>
            li.Description ?? $"{li.OfferingSnapshot.Subject.SubjectName} — Grade {(int)li.OfferingSnapshot.Grade}";

        public static string LearnerName(InvoiceLineItem li) =>
            $"{li.Learner.User.FirstName} {li.Learner.User.LastName}";

        public static string Month(InvoiceLineItem li) =>
            li.DueDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);

        // The lines a payment actually settled.
        public static List<InvoiceLineItem> PaidLines(Payment p) =>
            p.PaymentInvoices
                .SelectMany(pi => pi.Invoice.LineItems.Where(li => li.Status == LineItemStatus.Paid))
                .OrderBy(li => li.Learner.User.FirstName).ThenBy(li => li.DueDate)
                .ToList();

        public static string Learners(IEnumerable<InvoiceLineItem> lines) =>
            string.Join(", ", lines.Select(LearnerName).Distinct());

        public static string Money(decimal amount) =>
            "R" + amount.ToString("N2", CultureInfo.InvariantCulture);

        // Plain decimal for data-* attributes read by JavaScript (always a dot, never a comma).
        public static string Plain(decimal amount) =>
            amount.ToString("0.00", CultureInfo.InvariantCulture);

        public static string When(DateTime? utc) =>
            utc.HasValue ? utc.Value.ToLocalTime().ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture) : "—";
    }
}