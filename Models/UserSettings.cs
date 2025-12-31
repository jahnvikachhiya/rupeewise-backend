namespace ExpenseManagementAPI.Models
{
    public class UserSettings
    {
        public int UserId { get; set; }
        public string Theme { get; set; } = "system";
        public bool NotificationsEnabled { get; set; } = true;
        public bool ExpenseAlerts { get; set; } = true;
        public bool BudgetAlerts { get; set; } = true;
        public bool HideAmounts { get; set; } = false;
        public int MonthStartDay { get; set; } = 1;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}