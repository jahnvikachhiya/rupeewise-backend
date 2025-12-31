namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Budget entity - Maps to Budgets table in database
    /// Table Columns: BudgetId, UserId, CategoryId, BudgetAmount, MonthYear, CreatedAt, UpdatedAt
    /// Foreign Keys: UserId → Users.UserId, CategoryId → Categories.CategoryId
    /// Supports: Overall monthly budget (CategoryId = NULL) and Category-specific budgets
    /// </summary>
    public class Budget
    {
        /// <summary>
        /// BudgetId (INT, Primary Key, Identity)
        /// </summary>
        public int BudgetId { get; set; }

        /// <summary>
        /// UserId (INT, NOT NULL, Foreign Key → Users.UserId)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// CategoryId (INT, NULL, Foreign Key → Categories.CategoryId)
        /// NULL = Overall budget, Value = Category-specific budget
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// BudgetAmount (DECIMAL(18,2), NOT NULL)
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// MonthYear (NVARCHAR(7), NOT NULL)
        /// Format: 'YYYY-MM' (e.g., '2024-12')
        /// </summary>
        public string MonthYear { get; set; } = string.Empty;

        /// <summary>
        /// CreatedAt (DATETIME, DEFAULT GETDATE())
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UpdatedAt (DATETIME, DEFAULT GETDATE())
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (loaded from JOIN queries, not database columns)
        public string? CategoryName { get; set; }
        public string? UserFullName { get; set; }

        // Calculated properties (computed at runtime, not stored in database)
        public decimal CurrentSpending { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal PercentageUsed { get; set; }
        public string? BudgetStatus { get; set; }
    }
}