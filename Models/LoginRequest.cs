using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Request model for POST /api/users/login
    /// User can login with username OR email
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Username or Email
        /// </summary>
        [Required(ErrorMessage = "Username or Email is required")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        /// <summary>
        /// Password (plain text - will be hashed for comparison)
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for successful login
    /// Returns JWT token and user information
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; } = new UserInfo();
    }

    /// <summary>
    /// User information returned after successful login
    /// </summary>
    public class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
    }
}