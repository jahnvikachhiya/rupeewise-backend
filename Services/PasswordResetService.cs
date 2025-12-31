using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;
using ExpenseManagementAPI.Models;
using System.Security.Cryptography;
using ExpenseManagementAPI.Helpers;

namespace ExpenseManagementAPI.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IPasswordResetRepository _resetRepository;
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public PasswordResetService(
            IPasswordResetRepository resetRepository,
            IUserRepository userRepository,
            PasswordHasher passwordHasher,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _resetRepository = resetRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<(bool success, string message)> SendResetEmailAsync(ForgotPasswordRequest request)
        {
            try
            {
                // Find user by email
                var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.Email);

                // IMPORTANT: Always return success message (don't reveal if email exists)
                if (user == null)
                {
                    return (true, "If your email exists, you will receive a password reset OTP.");
                }

                // Delete old tokens for this user
                await _resetRepository.DeleteExpiredTokensAsync(user.UserId);

                // Generate 6-digit OTP
                     var otp = GenerateOTP();

                // USE IST, NOT UTC
                var expiresAt = DateTime.Now.AddMinutes(2);
                var resetToken = new PasswordResetToken
                {
                    UserId = user.UserId,
                    Token = otp, // Store OTP as token
                    ExpiresAt = expiresAt,
                    Used = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _resetRepository.CreateTokenAsync(resetToken);

                // Send OTP email
                var emailSent = await _emailService.SendPasswordResetOTPAsync(user.Email, otp);
                
                if (!emailSent)
                {
                    Console.WriteLine($"Failed to send email to {user.Email}");
                }

                Console.WriteLine($"[DEV] OTP for {user.Email}: {otp}"); // For testing

                return (true, "If your email exists, you will receive a password reset OTP.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendResetEmailAsync: {ex.Message}");
                return (false, "Failed to process request");
            }
        }

        public async Task<(bool success, string message)> VerifyOTPAsync(VerifyOTPRequest request)
        {
            try
            {
                // Find user by email
                var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.Email);
                if (user == null)
                {
                    return (false, "Invalid OTP or email");
                }

                // Get the latest valid token for this user
              var resetToken = await _resetRepository.GetValidTokenAsync(user.UserId, request.OTP);
       
                if (resetToken == null || resetToken.UserId != user.UserId)
                {
                    return (false, "Invalid or expired OTP");
                }

                return (true, "OTP verified successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VerifyOTPAsync: {ex.Message}");
                return (false, "Failed to verify OTP");
            }
        }

        public async Task<(bool success, string message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                // Find user by email
                var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.Email);
                if (user == null)
                {
                    return (false, "Invalid request");
                }

         var resetToken = await _resetRepository.GetValidTokenAsync(user.UserId, request.OTP);
                if (resetToken == null || resetToken.UserId != user.UserId)
                {
                    return (false, "Invalid or expired OTP");
                }

                // Hash new password
                var passwordHash = _passwordHasher.HashPassword(request.NewPassword);

                // Update user password
                var success = await _userRepository.ChangePasswordAsync(user.UserId, passwordHash);
                if (!success)
                {
                    return (false, "Failed to update password");
                }

                // Mark OTP as used
                await _resetRepository.MarkTokenAsUsedAsync(resetToken.Id);

                return (true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ResetPasswordAsync: {ex.Message}");
                return (false, "Failed to reset password");
            }
        }

        // Generate 6-digit OTP
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit OTP
        }
    }
}