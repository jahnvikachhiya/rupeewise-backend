using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    public interface IUserSettingsService
    {
        Task<(bool success, string message, UserSettingsDTO? data)> GetUserSettingsAsync(int userId);
        Task<(bool success, string message)> UpdateUserSettingsAsync(int userId, UpdateUserSettingsRequest request);
    }
}