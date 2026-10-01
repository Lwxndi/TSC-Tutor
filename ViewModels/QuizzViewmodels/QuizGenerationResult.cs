namespace Tutor_Manager.ViewModels.QuizzViewmodels
{
    public class QuizGenerationResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }

        public static QuizGenerationResult Success() => new() { Succeeded = true };
        public static QuizGenerationResult Failure(string error) => new() { Succeeded = false, ErrorMessage = error };
    }
}