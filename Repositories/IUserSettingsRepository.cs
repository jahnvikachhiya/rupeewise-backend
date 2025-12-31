using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    public interface IUserSettingsRepository
    {
        Task<UserSettings?> GetUserSettingsAsync(int userId);
        Task<bool> CreateDefaultSettingsAsync(int userId);
        Task<bool> UpdateUserSettingsAsync(UserSettings settings);
        Task<bool> UserSettingsExistAsync(int userId);
    }
}