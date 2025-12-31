using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Request model for POST /api/users/register
    /// Email-based admin assignment: If email matches AdminEmails in appsettings.json, Role = 'Admin'
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Username (must be unique)
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email (must be unique)
        /// Used for admin role assignment
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password (will be hashed using BCrypt before storing)
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirm password (must match Password)
        /// </summary>
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Full name
        /// </summary>
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 200 characters")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Phone number (optional)
        /// </summary>
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Response model for successful registration
    /// </summary>
    public class RegisterResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = "Registration successful!";
    }

    /// <summary>
    /// Request model for PUT /api/users/change-password/{userId}
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for PUT /api/users/profile/{userId}
    /// </summary>
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(200, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
    }
}
