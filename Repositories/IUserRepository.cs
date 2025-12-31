using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// User Repository Interface
    /// Defines all user-related database operations
    /// </summary>
    public interface IUserRepository
    {
        // POST /api/users/register
        Task<int> CreateUserAsync(User user);

        // POST /api/users/login
        Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);

        // GET /api/users/profile/{userId}
        Task<User?> GetUserByIdAsync(int userId);

        // GET /api/users (Admin only)
        Task<List<User>> GetAllUsersAsync();

        // PUT /api/users/profile/{userId}
        Task<bool> UpdateUserProfileAsync(int userId, string fullName, string email, string? phoneNumber);

        // PUT /api/users/change-password/{userId}
        Task<bool> ChangePasswordAsync(int userId, string passwordHash);

        // PUT /api/users/deactivate/{userId} (Admin only)
        Task<bool> DeactivateUserAsync(int userId);

        // Validation helpers
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> EmailExistsForOtherUserAsync(string email, int userId);
        Task<bool> ActivateUserAsync(int userId);


        
    }
}