using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Expense Service Implementation
    /// Handles expense management business logic
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly INotificationService _notificationService;
        private readonly IBudgetService _budgetService;

        public ExpenseService(IExpenseRepository expenseRepository, INotificationService notificationService, IBudgetService budgetService)
        {
            _expenseRepository = expenseRepository;
            _notificationService = notificationService;
            _budgetService = budgetService;
        }

        /// <summary>
        /// POST /api/expenses - Create new expense
        /// Triggers: Budget alert check, Expense confirmation notification
        /// </summary>
        public async Task<(bool success, string message, ExpenseDTO? data)> CreateExpenseAsync(int userId, CreateExpenseDTO request)
        {
            try
            {
                var expense = new Expense
                {
                    UserId = userId,
                    CategoryId = request.CategoryId,
                    Amount = request.Amount,
                    ExpenseDate = request.ExpenseDate,
                    Description = request.Description,
                    PaymentMethod = request.PaymentMethod,
                    Status = "Approved",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                int expenseId = await _expenseRepository.CreateExpenseAsync(expense);

                if (expenseId > 0)
                {
                    expense.ExpenseId = expenseId;

                    // Send expense added confirmation notification
                    await _notificationService.SendExpenseAddedNotificationAsync(userId, expense.Amount, request.CategoryId, expenseId);

                    // Check budget and send alerts if needed
                    string monthYear = expense.ExpenseDate.ToString("yyyy-MM");
                    await _budgetService.CheckBudgetAndSendAlertsAsync(userId, request.CategoryId, monthYear);

                    // Get created expense with full details
                    var createdExpense = await _expenseRepository.GetExpenseByIdAsync(expenseId);
                    if (createdExpense != null)
                    {
                        var expenseDto = MapToExpenseDTO(createdExpense);
                        return (true, "Expense created successfully", expenseDto);
                    }

                    return (true, "Expense created successfully", null);
                }

                return (false, "Failed to create expense", null);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/expenses/my/{userId} - Get user's own expenses
        /// </summary>
        public async Task<(bool success, string message, List<ExpenseDTO>? data)> GetMyExpensesAsync(int userId)
        {
            try
            {
                var expenses = await _expenseRepository.GetExpensesByUserIdAsync(userId);
                var expenseDtos = expenses.Select(MapToExpenseDTO).ToList();
                return (true, "Expenses retrieved successfully", expenseDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/expenses/{expenseId} - Get expense by ID
        /// Access: Owner OR Admin
        /// </summary>
        public async Task<(bool success, string message, ExpenseDTO? data)> GetExpenseByIdAsync(int expenseId, int requestingUserId, string userRole)
        {
            try
            {
                var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);

                if (expense == null)
                {
                    return (false, "Expense not found", null);
                }

                // Check access: Owner or Admin
                if (expense.UserId != requestingUserId && userRole != "Admin")
                {
                    return (false, "You don't have permission to view this expense", null);
                }

                var expenseDto = MapToExpenseDTO(expense);
                return (true, "Expense retrieved successfully", expenseDto);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// PUT /api/expenses/{expenseId} - Update expense
        /// Access: Owner OR Admin
        /// </summary>
        public async Task<(bool success, string message)> UpdateExpenseAsync(int expenseId, int requestingUserId, string userRole, UpdateExpenseDTO request)
        {
            try
            {
                var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);

                if (expense == null)
                {
                    return (false, "Expense not found");
                }

                // Check access: Owner or Admin
                if (expense.UserId != requestingUserId && userRole != "Admin")
                {
                    return (false, "You don't have permission to update this expense");
                }

                expense.CategoryId = request.CategoryId;
                expense.Amount = request.Amount;
                expense.ExpenseDate = request.ExpenseDate;
                expense.Description = request.Description;
                expense.PaymentMethod = request.PaymentMethod;
                expense.UpdatedAt = DateTime.UtcNow;

                bool updated = await _expenseRepository.UpdateExpenseAsync(expense);

                if (updated)
                {
                    // Re-check budget after expense update
                    string monthYear = expense.ExpenseDate.ToString("yyyy-MM");
                    await _budgetService.CheckBudgetAndSendAlertsAsync(expense.UserId, request.CategoryId, monthYear);

                    return (true, "Expense updated successfully");
                }

                return (false, "Failed to update expense");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE /api/expenses/{expenseId} - Delete expense
        /// Access: Owner OR Admin
        /// </summary>
        public async Task<(bool success, string message)> DeleteExpenseAsync(int expenseId, int requestingUserId, string userRole)
        {
            try
            {
                var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId);

                if (expense == null)
                {
                    return (false, "Expense not found");
                }

                // Check access: Owner or Admin
                if (expense.UserId != requestingUserId && userRole != "Admin")
                {
                    return (false, "You don't have permission to delete this expense");
                }

                bool deleted = await _expenseRepository.DeleteExpenseAsync(expenseId);

                if (deleted)
                {
                    return (true, "Expense deleted successfully");
                }

                return (false, "Failed to delete expense");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// GET /api/expenses/all - Get all expenses (Admin only)
        /// </summary>
        public async Task<(bool success, string message, List<ExpenseDTO>? data)> GetAllExpensesAsync()
        {
            try
            {
                var expenses = await _expenseRepository.GetAllExpensesAsync();
                var expenseDtos = expenses.Select(MapToExpenseDTO).ToList();
                return (true, "All expenses retrieved successfully", expenseDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/expenses/filter - Filter expenses by multiple criteria
        /// </summary>
        public async Task<(bool success, string message, List<ExpenseDTO>? data)> FilterExpensesAsync(int userId, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount, string? paymentMethod)
        {
            try
            {
                var expenses = await _expenseRepository.FilterExpensesAsync(userId, categoryId, startDate, endDate, minAmount, maxAmount, paymentMethod);
                var expenseDtos = expenses.Select(MapToExpenseDTO).ToList();
                return (true, "Filtered expenses retrieved successfully", expenseDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/expenses/search - Search expenses by description or amount
        /// </summary>
        public async Task<(bool success, string message, List<ExpenseDTO>? data)> SearchExpensesAsync(int userId, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return (false, "Search term is required", null);
                }

                var expenses = await _expenseRepository.SearchExpensesAsync(userId, searchTerm);
                var expenseDtos = expenses.Select(MapToExpenseDTO).ToList();
                return (true, "Search results retrieved successfully", expenseDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Helper: Map Expense to ExpenseDTO
        /// </summary>
        private ExpenseDTO MapToExpenseDTO(Expense expense)
        {
            return new ExpenseDTO
            {
                ExpenseId = expense.ExpenseId,
                UserId = expense.UserId,
                CategoryId = expense.CategoryId,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,
                PaymentMethod = expense.PaymentMethod,
                Status = expense.Status,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt,
                CategoryName = expense.CategoryName ?? string.Empty,
                UserFullName = expense.UserFullName ?? string.Empty,
                 Username = expense.Username ?? string.Empty 
            };
        }
    }
}