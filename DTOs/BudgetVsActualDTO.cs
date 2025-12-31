namespace ExpenseManagementAPI.DTOs
{
    /// <summary>
    /// DTO for GET /api/budgets/vs-actual/{userId}?categoryId={categoryId}&monthYear={monthYear}
    /// Compares budgeted amount with actual spending
    /// </summary>
    public class BudgetVsActualDTO
    {
        public int? BudgetId { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "Overall";
        public string MonthYear { get; set; } = string.Empty;

        // Budget information
        public decimal BudgetAmount { get; set; }
        public bool HasBudget { get; set; }

        // Actual spending
        public decimal ActualSpending { get; set; }
        public int ExpenseCount { get; set; }

        // Comparison
        public decimal Difference { get; set; }
        public decimal PercentageUsed { get; set; }
        public bool IsOverBudget { get; set; }
        public string Status { get; set; } = "On Track";
    }

    /// <summary>
    /// DTO for GET /api/budgets/status/{userId}?categoryId={categoryId}&monthYear={monthYear}
    /// Checks budget status and percentage used
    /// Triggers alerts at 80% (Info), 90% (Warning), 100% (Alert)
    /// </summary>
    public class BudgetStatusDTO
    {
        public int? BudgetId { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "Overall";
        public string MonthYear { get; set; } = string.Empty;

        public decimal BudgetAmount { get; set; }
        public decimal CurrentSpending { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal PercentageUsed { get; set; }

        public string Status { get; set; } = "On Track";
        public string AlertLevel { get; set; } = "None";
        public bool ShouldAlert { get; set; }
        public string? AlertMessage { get; set; }
    }

    /// <summary>
    /// DTO for GET /api/reports/monthly/{userId}?month={month}&year={year}
    /// Generates monthly expense report
    /// </summary>
    public class MonthlyReportDTO
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthYear { get; set; } = string.Empty;

        // Summary
        public decimal TotalExpenses { get; set; }
        public int TotalExpenseCount { get; set; }

        // Budget comparison
        public decimal? BudgetAmount { get; set; }
        public decimal? BudgetRemaining { get; set; }
        public decimal? BudgetPercentageUsed { get; set; }

        // Category breakdown
        public List<CategoryBreakdownDTO> CategoryBreakdown { get; set; } = new();

        // Top expenses
        public List<ExpenseDTO> TopExpenses { get; set; } = new();
    }
}