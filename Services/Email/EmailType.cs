namespace Tutor_Manager.Services.Email
{
    public enum EmailType
    {
        RegistrationConfirmation,
        GuardianLinked,
        
        GuardianUnlinked,
        SessionBooked,
        SessionReminder,
        SessionCancelled,
        PaymentReceived,
        PaymentFailed,

        // --- Tutor Application ---
        TutorApplicationRejected,
        TutorApplicationChangesRequired,
        TutorApplicationApproved,

        // --- Admin Control --
        AdminAccountCreated,

        // --- Offerings ---
        OfferingAssigned,
        OfferingDeactivated
    }
}
