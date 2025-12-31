namespace ExpenseManagementAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for User
    /// Used in GET /api/users/profile/{userId} and GET /api/users (Admin only)
    /// Returns user data without sensitive information (password hash)
    /// </summary>
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        
    }
}