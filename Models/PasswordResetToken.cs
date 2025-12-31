namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Password Reset Token entity
    /// Maps to password_reset_tokens table
    /// </summary>
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}