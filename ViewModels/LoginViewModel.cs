using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public required string Email { get; set; }

        // No strength regex here on purpose - login only needs to CHECK the
        // password against the stored hash, not enforce complexity rules again.
        // Those rules only apply when a password is being SET (registration/reset).
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
