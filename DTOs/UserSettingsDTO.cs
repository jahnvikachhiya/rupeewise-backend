using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementAPI.DTOs
{
    public class UserSettingsDTO
    {
        public int UserId { get; set; }
        public string Theme { get; set; } = "system";
        public bool NotificationsEnabled { get; set; } = true;
        public bool ExpenseAlerts { get; set; } = true;
        public bool BudgetAlerts { get; set; } = true;
        public bool HideAmounts { get; set; } = false;
        public int MonthStartDay { get; set; } = 1;
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateUserSettingsRequest
    {
        [RegularExpression("^(light|dark|system)$", ErrorMessage = "Theme must be light, dark, or system")]
        public string? Theme { get; set; }

        public bool? NotificationsEnabled { get; set; }

        public bool? ExpenseAlerts { get; set; }

        public bool? BudgetAlerts { get; set; }

        public bool? HideAmounts { get; set; }

        [Range(1, 28, ErrorMessage = "Month start day must be between 1 and 28")]
        public int? MonthStartDay { get; set; }
    }
}