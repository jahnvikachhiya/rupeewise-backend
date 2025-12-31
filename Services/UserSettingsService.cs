using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;
using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserSettingsRepository _settingsRepository;

        public UserSettingsService(IUserSettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<(bool success, string message, UserSettingsDTO? data)> GetUserSettingsAsync(int userId)
        {
            try
            {
                var settings = await _settingsRepository.GetUserSettingsAsync(userId);

                // Create default settings if not exist
                if (settings == null)
                {
                    await _settingsRepository.CreateDefaultSettingsAsync(userId);
                    settings = await _settingsRepository.GetUserSettingsAsync(userId);
                }

                if (settings == null)
                {
                    return (false, "Failed to retrieve settings", null);
                }

                var dto = new UserSettingsDTO
                {
                    UserId = settings.UserId,
                    Theme = settings.Theme,
                    NotificationsEnabled = settings.NotificationsEnabled,
                    ExpenseAlerts = settings.ExpenseAlerts,
                    BudgetAlerts = settings.BudgetAlerts,
                    HideAmounts = settings.HideAmounts,
                    MonthStartDay = settings.MonthStartDay,
                    UpdatedAt = settings.UpdatedAt
                };

                return (true, "Settings retrieved successfully", dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserSettingsAsync: {ex.Message}");
                return (false, "Failed to retrieve settings", null);
            }
        }

        public async Task<(bool success, string message)> UpdateUserSettingsAsync(int userId, UpdateUserSettingsRequest request)
        {
            try
            {
                // Get existing settings
                var settings = await _settingsRepository.GetUserSettingsAsync(userId);

                // Create default if not exist
                if (settings == null)
                {
                    await _settingsRepository.CreateDefaultSettingsAsync(userId);
                    settings = await _settingsRepository.GetUserSettingsAsync(userId);
                }

                if (settings == null)
                {
                    return (false, "Failed to update settings");
                }

                // Update only provided fields
                if (request.Theme != null) settings.Theme = request.Theme;
                if (request.NotificationsEnabled.HasValue) settings.NotificationsEnabled = request.NotificationsEnabled.Value;
                if (request.ExpenseAlerts.HasValue) settings.ExpenseAlerts = request.ExpenseAlerts.Value;
                if (request.BudgetAlerts.HasValue) settings.BudgetAlerts = request.BudgetAlerts.Value;
                if (request.HideAmounts.HasValue) settings.HideAmounts = request.HideAmounts.Value;
                if (request.MonthStartDay.HasValue) settings.MonthStartDay = request.MonthStartDay.Value;

                var success = await _settingsRepository.UpdateUserSettingsAsync(settings);

                if (success)
                {
                    return (true, "Settings updated successfully");
                }

                return (false, "Failed to update settings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateUserSettingsAsync: {ex.Message}");
                return (false, "Failed to update settings");
            }
        }
    }
}