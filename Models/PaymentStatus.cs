namespace Tutor_Manager.Models.Enums
{
    // Invoice-level state. Previously "paid" was inferred from amounts; now it is explicit.
    public enum InvoiceStatus
    {
        Open = 0,            // payable
        Paid = 1,
        CarriedForward = 2,  // rolled into a later invoice; can no longer be paid on its own
        Voided = 3           // cancelled before payment (enrollment cancelled / redirected)
    }

    // One Stripe Checkout attempt (a "transaction").
    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Expired = 2,
        Failed = 3,
        NeedsReview = 4      // money arrived but did not match what we invoiced; nothing was applied
    }
}