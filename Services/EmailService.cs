using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;

namespace ExpenseManagementAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly TransactionalEmailsApi _apiInstance;
public EmailService(IConfiguration configuration)
{
    _configuration = configuration;

    var apiKey = _configuration["Brevo:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("Brevo API Key is not configured");
    }

    Configuration.Default.ApiKey["api-key"] = apiKey;
    _apiInstance = new TransactionalEmailsApi();
}

        public async Task<bool> SendPasswordResetOTPAsync(string toEmail, string otp)
        {
            try
            {
                var senderEmail = _configuration["Brevo:SenderEmail"];
                var senderName = _configuration["Brevo:SenderName"];

                // Create email
                var sendSmtpEmail = new SendSmtpEmail
                {
                    Sender = new SendSmtpEmailSender(senderName, senderEmail),
                    To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(toEmail) },
                    Subject = "Password Reset OTP - RupeeWise",
                    HtmlContent = GetPasswordResetOTPTemplate(otp)
                };

                // Send email
                var result = await _apiInstance.SendTransacEmailAsync(sendSmtpEmail);
                
                Console.WriteLine($"OTP email sent successfully to {toEmail}. Message ID: {result.MessageId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OTP email: {ex.Message}");
                return false;
            }
        }

        private string GetPasswordResetOTPTemplate(string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #2E7D32 0%, #4CAF50 100%); padding: 40px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 32px;'>🔐 RupeeWise</h1>
                            <p style='color: #E8F5E9; margin: 10px 0 0 0; font-size: 16px;'>Secure Expense Management</p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px;'>
                            <h2 style='color: #333333; margin-top: 0;'>Password Reset Request</h2>
                            
                            <p style='color: #666666; font-size: 16px; line-height: 1.6;'>
                                You requested to reset your password. Use the OTP code below to verify your identity:
                            </p>
                            
                            <!-- OTP Code Box -->
                            <table width='100%' cellpadding='0' cellspacing='0' style='margin: 30px 0;'>
                                <tr>
                                    <td align='center'>
                                        <div style='background: linear-gradient(135deg, #2E7D32 0%, #4CAF50 100%); 
                                                    padding: 30px; 
                                                    border-radius: 10px;
                                                    box-shadow: 0 4px 15px rgba(46, 125, 50, 0.3);'>
                                            <p style='color: #E8F5E9; margin: 0 0 10px 0; font-size: 14px; text-transform: uppercase; letter-spacing: 2px;'>
                                                Your OTP Code
                                            </p>
                                            <h1 style='color: white; 
                                                       margin: 0; 
                                                       font-size: 48px; 
                                                       letter-spacing: 10px; 
                                                       font-weight: bold;
                                                       font-family: monospace;'>
                                                {otp}
                                            </h1>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Instructions -->
                            <p style='color: #666666; font-size: 16px; line-height: 1.6; text-align: center;'>
                                Enter this code on the password reset page to continue.
                            </p>
                            
                           <!-- Warning -->
                                <div style='background-color: #FFF3E0; border-left: 4px solid #FF9800; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                                    <p style='margin: 0; color: #E65100; font-weight: bold; font-size: 14px;'>
                                        ⏱️ This code expires in 2 minutes
                                    </p>
                                </div>
                                                            
                            <p style='color: #666666; font-size: 14px; line-height: 1.6;'>
                                If you didn't request a password reset, please ignore this email or contact support if you have concerns about your account security.
                            </p>
                            
                            <!-- Security Tips -->
                            <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #E0E0E0;'>
                                <p style='color: #999999; font-size: 12px; margin: 0;'>
                                    <strong>Security Tips:</strong><br>
                                    • Never share this OTP with anyone<br>
                                    • RupeeWise will never ask for your OTP via phone or email<br>
                                    • If you didn't request this, ignore this email
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f9f9f9; padding: 30px; text-align: center; border-top: 1px solid #eeeeee;'>
                            <p style='margin: 0; color: #999999; font-size: 12px;'>
                                RupeeWise - Secure Expense Management<br>
                                This is an automated email, please do not reply.
                            </p>
                        </td>
                    </tr>
                    
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}