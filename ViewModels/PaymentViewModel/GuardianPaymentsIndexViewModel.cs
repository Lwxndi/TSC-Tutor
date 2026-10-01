using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.ViewModels
{
    // Guardian Payments page: unpaid invoices (with checkboxes) + the transactions table.
    public class GuardianPaymentsIndexViewModel
    {
        public List<Invoice> PayableInvoices { get; set; } = new();
        public List<Payment> Transactions { get; set; } = new();
    }

    // Success page. Status is what Stripe/our records say right now:
    //   Paid    -> "Paid, thank you" + receipt button
    //   Pending -> "Still processing" (page can be refreshed)
    public class PaymentSuccessViewModel
    {
        public Payment Payment { get; set; } = null!;
        public PaymentStatus Status { get; set; }
        public bool IsPaid => Status == PaymentStatus.Paid;
    }

    public class AdminPaymentsIndexViewModel
    {
        public string? Search { get; set; }
        public List<Payment> Payments { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
    }
}