using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;
namespace ExpenseManagementAPI.Repositories;


    /// <summary>
    /// Expense Repository Implementation
    /// Handles all expense-related database operations using ADO.NET
    /// </summary>
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public ExpenseRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// POST /api/expenses - Create new expense
        /// </summary>
        public async Task<int> CreateExpenseAsync(Expense expense)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", expense.UserId },
                { "@CategoryId", expense.CategoryId },
                { "@Amount", expense.Amount },
                { "@ExpenseDate", expense.ExpenseDate },
                { "@Description", expense.Description ?? (object)DBNull.Value },
                { "@PaymentMethod", expense.PaymentMethod },
                { "@Status", expense.Status }
            };

            return await Task.Run(() => _dbHelper.ExecuteInsertWithId(SqlQueryHelper.InsertExpense, parameters));
        }

        /// <summary>
        /// GET /api/expenses/my/{userId} - Get all expenses for a user
        /// </summary>
        public async Task<List<Expense>> GetExpensesByUserIdAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() =>
            {
                var expenses = new List<Expense>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetExpensesByUserId, parameters);
                while (reader.Read())
                {
                    expenses.Add(MapExpenseFromReader(reader));
                }
                return expenses;
            });
        }

        /// <summary>
        /// GET /api/expenses/{expenseId} - Get expense by ID
        /// </summary>
        public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@ExpenseId", expenseId }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetExpenseById, parameters);
                if (reader.Read())
                {
                    return MapExpenseFromReader(reader);
                }
                return null;
            });
        }

        /// <summary>
        /// PUT /api/expenses/{expenseId} - Update expense
        /// </summary>
        public async Task<bool> UpdateExpenseAsync(Expense expense)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@ExpenseId", expense.ExpenseId },
                { "@CategoryId", expense.CategoryId },
                { "@Amount", expense.Amount },
                { "@ExpenseDate", expense.ExpenseDate },
                { "@Description", expense.Description ?? (object)DBNull.Value },
                { "@PaymentMethod", expense.PaymentMethod }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.UpdateExpense, parameters) > 0);
        }

        /// <summary>
        /// DELETE /api/expenses/{expenseId} - Delete expense
        /// </summary>
        public async Task<bool> DeleteExpenseAsync(int expenseId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@ExpenseId", expenseId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.DeleteExpense, parameters) > 0);
        }

        /// <summary>
        /// GET /api/expenses/all - Get all expenses (Admin only)
        /// </summary>
        public async Task<List<Expense>> GetAllExpensesAsync()
        {
            return await Task.Run(() =>
            {
                var expenses = new List<Expense>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetAllExpenses);
                while (reader.Read())
                {
                    expenses.Add(MapExpenseFromReader(reader));
                }
                return expenses;
            });
        }

        /// <summary>
        /// GET /api/expenses/filter - Filter expenses by multiple criteria
        /// </summary>
 public async Task<List<Expense>> FilterExpensesAsync(
    int? userId,
    int? categoryId,
    DateTime? startDate,
    DateTime? endDate,
    decimal? minAmount,
    decimal? maxAmount,
    string? paymentMethod
)
{
    var parameters = new Dictionary<string, object>
    {
        { "@UserId", (object?)userId ?? DBNull.Value },
        { "@CategoryId", (object?)categoryId ?? DBNull.Value },
        { "@StartDate", (object?)startDate ?? DBNull.Value },
        { "@EndDate", (object?)endDate ?? DBNull.Value },
        { "@MinAmount", (object?)minAmount ?? DBNull.Value },
        { "@MaxAmount", (object?)maxAmount ?? DBNull.Value },
        { "@PaymentMethod", string.IsNullOrEmpty(paymentMethod) ? DBNull.Value : paymentMethod }
    };

    var expenses = new List<Expense>();
    
    using (var reader = _dbHelper.ExecuteReader(SqlQueryHelper.FilterExpenses, parameters))
    {
        while (reader.Read())
        {
            expenses.Add(MapExpenseFromReader(reader));
        }
    }

    return expenses;
}



        /// <summary>
        /// GET /api/expenses/search - Search expenses by description or amount
        /// </summary>
        public async Task<List<Expense>> SearchExpensesAsync(int userId, string searchTerm)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@SearchTerm", searchTerm }
            };

            return await Task.Run(() =>
            {
                var expenses = new List<Expense>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.SearchExpenses, parameters);
                while (reader.Read())
                {
                    expenses.Add(MapExpenseFromReader(reader));
                }
                return expenses;
            });
        }

        /// <summary>
        /// Get total expense count for a user
        /// </summary>
        public async Task<int> GetExpenseCountByUserAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.GetCount(SqlQueryHelper.GetExpenseCountByUser, parameters));
        }

        /// <summary>
        /// Get total spending for a user
        /// </summary>
        public async Task<decimal> GetTotalSpendingByUserAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.GetSum(SqlQueryHelper.GetTotalSpendingByUser, parameters));
        }

        /// <summary>
        /// Helper method to map SqlDataReader to Expense object
        /// </summary>
        private Expense MapExpenseFromReader(Microsoft.Data.SqlClient.SqlDataReader reader)
        {
            return new Expense
            {
                ExpenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                UserFullName = reader.GetString(reader.GetOrdinal("UserFullName")),
                Username = reader.GetString(reader.GetOrdinal("Username"))
            };
        }
    }
