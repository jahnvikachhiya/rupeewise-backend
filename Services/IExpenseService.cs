using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Expense Service Interface
    /// Defines business logic operations for expense management
    /// </summary>
    public interface IExpenseService
    {
        // POST /api/expenses
        Task<(bool success, string message, ExpenseDTO? data)> CreateExpenseAsync(int userId, CreateExpenseDTO request);

        // GET /api/expenses/my/{userId}
        Task<(bool success, string message, List<ExpenseDTO>? data)> GetMyExpensesAsync(int userId);

        // GET /api/expenses/{expenseId}
        Task<(bool success, string message, ExpenseDTO? data)> GetExpenseByIdAsync(int expenseId, int requestingUserId, string userRole);

        // PUT /api/expenses/{expenseId}
        Task<(bool success, string message)> UpdateExpenseAsync(int expenseId, int requestingUserId, string userRole, UpdateExpenseDTO request);

        // DELETE /api/expenses/{expenseId}
        Task<(bool success, string message)> DeleteExpenseAsync(int expenseId, int requestingUserId, string userRole);

        // GET /api/expenses/all (Admin only)
        Task<(bool success, string message, List<ExpenseDTO>? data)> GetAllExpensesAsync();

        // GET /api/expenses/filter
        Task<(bool success, string message, List<ExpenseDTO>? data)> FilterExpensesAsync(int userId, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, string? paymentMethod);

        // GET /api/expenses/search
        Task<(bool success, string message, List<ExpenseDTO>? data)> SearchExpensesAsync(int userId, string searchTerm);
    }
}