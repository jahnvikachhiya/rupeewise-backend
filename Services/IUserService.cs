using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// User Service Interface
    /// Defines business logic operations for user management
    /// </summary>
    public interface IUserService
    {
        // POST /api/users/register
        Task<(bool success, string message, RegisterResponse? data)> RegisterUserAsync(RegisterRequest request, List<string> adminEmails);

        // POST /api/users/login
        Task<(bool success, string message, LoginResponse? data)> LoginUserAsync(LoginRequest request);

        // GET /api/users/profile/{userId}
        Task<(bool success, string message, UserDTO? data)> GetUserProfileAsync(int userId);

        // GET /api/users (Admin only)
        Task<(bool success, string message, List<UserDTO>? data)> GetAllUsersAsync();

        // PUT /api/users/profile/{userId}
        Task<(bool success, string message)> UpdateUserProfileAsync(int userId, UpdateProfileRequest request);

        // PUT /api/users/change-password/{userId}
        Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordRequest request);

        // PUT /api/users/deactivate/{userId} (Admin only)
        Task<(bool success, string message)> DeactivateUserAsync(int userId);

        // Helper: Generate JWT Token
        string GenerateJwtToken(User user);

        Task<(bool success, string message)> ActivateUserAsync(int userId);

    }
}