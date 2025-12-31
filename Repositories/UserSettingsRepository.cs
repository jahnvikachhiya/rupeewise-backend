using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public UserSettingsRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<UserSettings?> GetUserSettingsAsync(int userId)
        {
            var query = @"
                SELECT user_id, theme, notifications_enabled, expense_alerts, 
                       budget_alerts, hide_amounts, month_start_day, updated_at
                FROM user_settings
                WHERE user_id = @UserId";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    return new UserSettings
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                        Theme = reader.GetString(reader.GetOrdinal("theme")),
                        NotificationsEnabled = reader.GetBoolean(reader.GetOrdinal("notifications_enabled")),
                        ExpenseAlerts = reader.GetBoolean(reader.GetOrdinal("expense_alerts")),
                        BudgetAlerts = reader.GetBoolean(reader.GetOrdinal("budget_alerts")),
                        HideAmounts = reader.GetBoolean(reader.GetOrdinal("hide_amounts")),
                        MonthStartDay = reader.GetInt32(reader.GetOrdinal("month_start_day")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
                    };
                }
                return null;
            });
        }

        public async Task<bool> CreateDefaultSettingsAsync(int userId)
        {
            var query = @"
                INSERT INTO user_settings (user_id, theme, notifications_enabled, expense_alerts, 
                                          budget_alerts, hide_amounts, month_start_day, updated_at)
                VALUES (@UserId, 'system', 1, 1, 1, 0, 1, SYSDATETIME())";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters) > 0);
        }

        public async Task<bool> UpdateUserSettingsAsync(UserSettings settings)
        {
            var query = @"
                UPDATE user_settings
                SET theme = @Theme,
                    notifications_enabled = @NotificationsEnabled,
                    expense_alerts = @ExpenseAlerts,
                    budget_alerts = @BudgetAlerts,
                    hide_amounts = @HideAmounts,
                    month_start_day = @MonthStartDay,
                    updated_at = SYSDATETIME()
                WHERE user_id = @UserId";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", settings.UserId },
                { "@Theme", settings.Theme },
                { "@NotificationsEnabled", settings.NotificationsEnabled },
                { "@ExpenseAlerts", settings.ExpenseAlerts },
                { "@BudgetAlerts", settings.BudgetAlerts },
                { "@HideAmounts", settings.HideAmounts },
                { "@MonthStartDay", settings.MonthStartDay }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters) > 0);
        }

        public async Task<bool> UserSettingsExistAsync(int userId)
        {
            var query = "SELECT COUNT(1) FROM user_settings WHERE user_id = @UserId";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.RecordExists(query, parameters));
        }
    }
}