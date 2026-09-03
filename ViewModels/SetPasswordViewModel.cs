// ViewModels/SetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class SetPasswordViewModel
    {
        [Required]
        public  string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public  string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public  string ConfirmPassword { get; set; } = string.Empty;
    }
}