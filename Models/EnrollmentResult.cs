namespace Tutor_Manager.Models
{
    public class EnrollmentResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public int? EnrollmentId { get; set; }
        public List<string> Warnings { get; set; } = new();

        public static EnrollmentResult Success(int id, List<string>? warnings = null) =>
            new() { Succeeded = true, EnrollmentId = id, Warnings = warnings ?? new() };

        public static EnrollmentResult Failure(string error) =>
            new() { Succeeded = false, ErrorMessage = error };
    }
}
