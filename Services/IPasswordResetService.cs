using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    public interface IPasswordResetService
    {
        Task<(bool success, string message)> SendResetEmailAsync(ForgotPasswordRequest request);
        Task<(bool success, string message)> VerifyOTPAsync(VerifyOTPRequest request);
        Task<(bool success, string message)> ResetPasswordAsync(ResetPasswordRequest request);
    }
}