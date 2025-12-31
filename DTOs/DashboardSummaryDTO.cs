namespace ExpenseManagementAPI.DTOs
{
    /// <summary>
    /// DTO for GET /api/dashboard/summary/{userId}
    /// Returns total expenses, monthly expenses, today's expenses
    /// </summary>
    public class DashboardSummaryDTO
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;

        // Total statistics (all-time)
        public decimal TotalExpenses { get; set; }
        public int TotalExpenseCount { get; set; }

        // Current month statistics
        public decimal MonthlyExpenses { get; set; }
        public int MonthlyExpenseCount { get; set; }

        // Today's statistics
        public decimal TodayExpenses { get; set; }
        public int TodayExpenseCount { get; set; }
    }

    /// <summary>
    /// DTO for GET /api/dashboard/category-breakdown/{userId}?startDate={startDate}&endDate={endDate}
    /// Returns category-wise spending breakdown
    /// </summary>
    public class CategoryBreakdownDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public decimal Amount { get; set; }
        public int ExpenseCount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    /// <summary>
    /// DTO for GET /api/dashboard/monthly-trend/{userId}?year={year}
    /// Returns monthly spending trend for a specific year
    /// </summary>
    public class MonthlyTrendDTO
    {
        public string MonthYear { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int ExpenseCount { get; set; }
    }

    /// <summary>
    /// DTO for GET /api/dashboard/budget-progress/{userId}?monthYear={monthYear}
    /// Returns budget progress for all categories in a specific month
    /// </summary>
    public class BudgetProgressDTO
    {
        public int? BudgetId { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "Overall";
        public decimal BudgetAmount { get; set; }
        public decimal CurrentSpending { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal PercentageUsed { get; set; }
        public string BudgetStatus { get; set; } = "On Track";
    }
}