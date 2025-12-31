using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for Budget
    /// Used in GET /api/budgets/my/{userId}, GET /api/budgets/{budgetId}
    /// </summary>
    public class BudgetDTO
    {
        public int BudgetId { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; }
        public decimal BudgetAmount { get; set; }
        public string MonthYear { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties from JOIN queries
        public string? CategoryName { get; set; }

        public string? CategoryIcon { get; set; }

        public string UserFullName { get; set; } = string.Empty;

        // Computed properties (calculated at runtime)
        public decimal CurrentSpending { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal PercentageUsed { get; set; }
        public string BudgetStatus { get; set; } = "On Track";
    }

    /// <summary>
    /// Request DTO for POST /api/budgets (Create/Set budget)
    /// </summary>
    public class CreateBudgetDTO
    {
        [Required(ErrorMessage = "Budget amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
        public decimal BudgetAmount { get; set; }

        [Required(ErrorMessage = "Month and year are required")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "MonthYear must be in format YYYY-MM")]
        public string MonthYear { get; set; } = string.Empty;

        // NULL for overall budget, CategoryId for category-specific budget
        public int? CategoryId { get; set; }
    }

    /// <summary>
    /// Request DTO for PUT /api/budgets/{budgetId} (Update budget)
    /// </summary>
    public class UpdateBudgetDTO
    {
        [Required(ErrorMessage = "Budget amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
        public decimal BudgetAmount { get; set; }
    }
}