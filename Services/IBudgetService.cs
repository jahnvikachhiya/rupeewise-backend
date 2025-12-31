using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Budget Service Interface
    /// Defines business logic operations for budget management
    /// </summary>
    public interface IBudgetService
    {
        // POST /api/budgets
        Task<(bool success, string message, BudgetDTO? data)> CreateBudgetAsync(int userId, CreateBudgetDTO request);

        // GET /api/budgets/my/{userId}?monthYear={monthYear}
        Task<(bool success, string message, List<BudgetDTO>? data)> GetMyBudgetsAsync(int userId, string monthYear);

        // GET /api/budgets/{budgetId}
        Task<(bool success, string message, BudgetDTO? data)> GetBudgetByIdAsync(int budgetId, int requestingUserId, string userRole);

        // PUT /api/budgets/{budgetId}
        Task<(bool success, string message)> UpdateBudgetAsync(int budgetId, int requestingUserId, string userRole, UpdateBudgetDTO request);

        // DELETE /api/budgets/{budgetId}
        Task<(bool success, string message)> DeleteBudgetAsync(int budgetId, int requestingUserId, string userRole);

        // GET /api/budgets/vs-actual/{userId}
        Task<(bool success, string message, BudgetVsActualDTO? data)> GetBudgetVsActualAsync(int userId, int? categoryId, string monthYear);

        // GET /api/budgets/status/{userId}
        Task<(bool success, string message, BudgetStatusDTO? data)> GetBudgetStatusAsync(int userId, int? categoryId, string monthYear);

        // POST /api/notifications/send-budget-alert - Manually trigger budget alert check
        Task CheckBudgetAndSendAlertsAsync(int userId, int? categoryId, string monthYear);
    }
}