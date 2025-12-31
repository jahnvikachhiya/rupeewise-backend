namespace ExpenseManagementAPI.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetOTPAsync(string toEmail, string otp);
    }
}