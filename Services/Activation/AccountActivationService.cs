// Services/Activation/AccountActivationService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Services.Activation
{
    public class AccountActivationService : IAccountActivationService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private const int TokenValidityHours = 48;

        public AccountActivationService(Tutor_ManagerDatabaseContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> GenerateTokenAsync(int userId)
        {
            var token = Guid.NewGuid().ToString("N"); // no dashes, cleaner URL

            _context.AccountActivationTokens.Add(new AccountActivationToken
            {
                Token = token,
                UserId = userId,
                ExpiryDate = DateTime.Now.AddHours(TokenValidityHours),
                IsUsed = false
            });

            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<AccountActivationResult> ValidateTokenAsync(string token)
        {
            var record = await _context.AccountActivationTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (record == null)
                return new AccountActivationResult { IsValid = false, ErrorMessage = "Invalid activation link." };

            if (record.IsUsed)
                return new AccountActivationResult { IsValid = false, ErrorMessage = "This activation link has already been used." };

            if (record.ExpiryDate < DateTime.Now)
                return new AccountActivationResult { IsValid = false, ErrorMessage = "This activation link has expired." };

            return new AccountActivationResult { IsValid = true, UserId = record.UserId };
        }

        public async Task<bool> ActivateAccountAsync(string token, string newPassword)
        {
            var validation = await ValidateTokenAsync(token);

            if (!validation.IsValid || validation.UserId == null)
                return false;

            var user = await _context.Users.FindAsync(validation.UserId.Value);
            if (user == null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.IsActive = true;

            var tutor = await _context.Tutors.FindAsync(user.UserId);
            if (tutor != null)
            {
                tutor.AccountStatus = AccountStatus.Active;
            }

            var tokenRecord = await _context.AccountActivationTokens
                .FirstOrDefaultAsync(t => t.Token == token);
            if (tokenRecord != null)
            {
                tokenRecord.IsUsed = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}