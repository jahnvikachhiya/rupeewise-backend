using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// Expense Repository Interface
    /// Defines all expense-related database operations
    /// </summary>
    public interface IExpenseRepository
    {
        // POST /api/expenses
        Task<int> CreateExpenseAsync(Expense expense);

        // GET /api/expenses/my/{userId}
        Task<List<Expense>> GetExpensesByUserIdAsync(int userId);

        // GET /api/expenses/{expenseId}
        Task<Expense?> GetExpenseByIdAsync(int expenseId);

        // PUT /api/expenses/{expenseId}
        Task<bool> UpdateExpenseAsync(Expense expense);

        // DELETE /api/expenses/{expenseId}
        Task<bool> DeleteExpenseAsync(int expenseId);

        // GET /api/expenses/all (Admin only)
        Task<List<Expense>> GetAllExpensesAsync();

        // GET /api/expenses/filter
        Task<List<Expense>> FilterExpensesAsync(int? userId, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, string? paymentMethod);

        // GET /api/expenses/search
        Task<List<Expense>> SearchExpensesAsync(int userId, string searchTerm);

        // Helper methods
        Task<int> GetExpenseCountByUserAsync(int userId);
        Task<decimal> GetTotalSpendingByUserAsync(int userId);
    }
}