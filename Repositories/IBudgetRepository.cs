using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// Budget Repository Interface
    /// Defines all budget-related database operations
    /// </summary>
    public interface IBudgetRepository
    {
        // POST /api/budgets
        Task<int> CreateBudgetAsync(Budget budget);

        // GET /api/budgets/my/{userId}?monthYear={monthYear}
        Task<List<Budget>> GetBudgetsByUserAndMonthAsync(int userId, string monthYear);

        // GET /api/budgets/{budgetId}
        Task<Budget?> GetBudgetByIdAsync(int budgetId);

        // PUT /api/budgets/{budgetId}
        Task<bool> UpdateBudgetAsync(int budgetId, decimal budgetAmount);

        // DELETE /api/budgets/{budgetId}
        Task<bool> DeleteBudgetAsync(int budgetId);

        // GET /api/budgets/vs-actual/{userId}
        Task<BudgetVsActualDTO?> GetBudgetVsActualAsync(int userId, int? categoryId, string monthYear);

        // GET /api/budgets/status/{userId}
        Task<BudgetStatusDTO?> GetBudgetStatusAsync(int userId, int? categoryId, string monthYear);

        // Helper methods
        Task<bool> BudgetExistsAsync(int userId, int? categoryId, string monthYear);
        Task<int?> GetBudgetIdIfExistsAsync(int userId, int? categoryId, string monthYear);
        Task<decimal> GetCurrentSpendingAsync(int userId, int? categoryId, string monthYear);
        Task<List<Budget>> GetAllBudgetsByUserAsync(int userId);
    }
}