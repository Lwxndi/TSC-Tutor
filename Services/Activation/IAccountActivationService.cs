// Services/Activation/IAccountActivationService.cs
namespace Tutor_Manager.Services.Activation
{
    public interface IAccountActivationService
    {
        Task<string> GenerateTokenAsync(int userId);
        Task<AccountActivationResult> ValidateTokenAsync(string token);
        Task<bool> ActivateAccountAsync(string token, string newPassword);
    }

    public class AccountActivationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int? UserId { get; set; }
    }
}