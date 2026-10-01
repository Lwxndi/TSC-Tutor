namespace Tutor_Manager.Options
{
    // Printed in the header of every receipt/invoice PDF. Bound from the "Business"
    // section of appsettings.json — not sensitive, so it doesn't belong in user secrets.
    public class BusinessOptions
    {
        public string Name { get; set; } = "The Science Community";
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}