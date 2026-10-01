namespace Tutor_Manager.Models.Enums
{
    // Learner's chosen mode for THIS enrollment — validated against
    // Offering.DeliveryMethod (see EnrollmentService.ValidateDeliveryMode).
    public enum DeliveryMode
    {
        InPerson,
        Remote
    }
}