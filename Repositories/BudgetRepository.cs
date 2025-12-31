using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// Budget Repository Implementation
    /// Handles all budget-related database operations using ADO.NET
    /// </summary>
    public class BudgetRepository : IBudgetRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public BudgetRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// POST /api/budgets - Create budget
        /// </summary>
        public async Task<int> CreateBudgetAsync(Budget budget)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", budget.UserId },
                { "@CategoryId", budget.CategoryId ?? (object)DBNull.Value },
                { "@BudgetAmount", budget.BudgetAmount },
                { "@MonthYear", budget.MonthYear }
            };

            return await Task.Run(() => _dbHelper.ExecuteInsertWithId(SqlQueryHelper.InsertBudget, parameters));
        }

        /// <summary>
        /// GET /api/budgets/my/{userId}?monthYear={monthYear}
        /// </summary>
        public async Task<List<Budget>> GetBudgetsByUserAndMonthAsync(int userId, string monthYear)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@MonthYear", monthYear }
            };

            return await Task.Run(() =>
            {
                var budgets = new List<Budget>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetBudgetsByUserAndMonth, parameters);
                while (reader.Read())
                {
                    budgets.Add(MapBudgetFromReader(reader));
                }
                return budgets;
            });
        }

        /// <summary>
        /// GET /api/budgets/{budgetId}
        /// </summary>
        public async Task<Budget?> GetBudgetByIdAsync(int budgetId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BudgetId", budgetId }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetBudgetById, parameters);
                if (reader.Read())
                {
                    return MapBudgetFromReader(reader);
                }
                return null;
            });
        }

        /// <summary>
        /// PUT /api/budgets/{budgetId} - Update budget
        /// </summary>
        public async Task<bool> UpdateBudgetAsync(int budgetId, decimal budgetAmount)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BudgetId", budgetId },
                { "@BudgetAmount", budgetAmount }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.UpdateBudget, parameters) > 0);
        }

        /// <summary>
        /// DELETE /api/budgets/{budgetId}
        /// </summary>
        public async Task<bool> DeleteBudgetAsync(int budgetId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BudgetId", budgetId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.DeleteBudget, parameters) > 0);
        }

        /// <summary>
        /// GET /api/budgets/vs-actual/{userId} - Budget vs Actual spending
        /// </summary>
        public async Task<BudgetVsActualDTO?> GetBudgetVsActualAsync(int userId, int? categoryId, string monthYear)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CategoryId", categoryId ?? (object)DBNull.Value },
                { "@MonthYear", monthYear }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetBudgetVsActual, parameters);
                if (reader.Read())
                {
                    var budgetAmount = reader.GetDecimal(reader.GetOrdinal("BudgetAmount"));
                    var actualSpending = reader.GetDecimal(reader.GetOrdinal("ActualSpending"));
                    var difference = budgetAmount - actualSpending;
                    var percentageUsed = budgetAmount > 0 ? (actualSpending / budgetAmount) * 100 : 0;

                    return new BudgetVsActualDTO
                    {
                        BudgetId = reader.IsDBNull(reader.GetOrdinal("BudgetId")) ? null : reader.GetInt32(reader.GetOrdinal("BudgetId")),
                        CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? "Overall" : reader.GetString(reader.GetOrdinal("CategoryName")),
                        MonthYear = reader.GetString(reader.GetOrdinal("MonthYear")),
                        BudgetAmount = budgetAmount,
                        HasBudget = true,
                        ActualSpending = actualSpending,
                        ExpenseCount = reader.GetInt32(reader.GetOrdinal("ExpenseCount")),
                        Difference = difference,
                        PercentageUsed = percentageUsed,
                        IsOverBudget = actualSpending > budgetAmount,
                        Status = GetBudgetStatus(percentageUsed, actualSpending > budgetAmount)
                    };
                }
                return null;
            });
        }

        /// <summary>
        /// GET /api/budgets/status/{userId} - Check budget status
        /// </summary>
        public async Task<BudgetStatusDTO?> GetBudgetStatusAsync(int userId, int? categoryId, string monthYear)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CategoryId", categoryId ?? (object)DBNull.Value },
                { "@MonthYear", monthYear }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetBudgetStatus, parameters);
                if (reader.Read())
                {
                    var budgetAmount = reader.GetDecimal(reader.GetOrdinal("BudgetAmount"));
                    var currentSpending = reader.GetDecimal(reader.GetOrdinal("CurrentSpending"));
                    var remaining = budgetAmount - currentSpending;
                    var percentageUsed = budgetAmount > 0 ? (currentSpending / budgetAmount) * 100 : 0;

                    var status = GetBudgetStatus(percentageUsed, currentSpending > budgetAmount);
                    var alertLevel = GetAlertLevel(percentageUsed);
                    var shouldAlert = percentageUsed >= 80;

                    return new BudgetStatusDTO
                    {
                        BudgetId = reader.IsDBNull(reader.GetOrdinal("BudgetId")) ? null : reader.GetInt32(reader.GetOrdinal("BudgetId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? "Overall" : reader.GetString(reader.GetOrdinal("CategoryName")),
                        MonthYear = reader.GetString(reader.GetOrdinal("MonthYear")),
                        BudgetAmount = budgetAmount,
                        CurrentSpending = currentSpending,
                        RemainingBudget = remaining,
                        PercentageUsed = percentageUsed,
                        Status = status,
                        AlertLevel = alertLevel,
                        ShouldAlert = shouldAlert,
                        AlertMessage = shouldAlert ? GetAlertMessage(percentageUsed, currentSpending, budgetAmount) : null
                    };
                }
                return null;
            });
        }

        /// <summary>
        /// Check if budget exists
        /// </summary>
        public async Task<bool> BudgetExistsAsync(int userId, int? categoryId, string monthYear)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CategoryId", categoryId ?? (object)DBNull.Value },
                { "@MonthYear", monthYear }
            };

            return await Task.Run(() => _dbHelper.RecordExists(SqlQueryHelper.CheckBudgetExists, parameters));
        }

        /// <summary>
        /// Get budget ID if exists
        /// </summary>
        public async Task<int?> GetBudgetIdIfExistsAsync(int userId, int? categoryId, string monthYear)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CategoryId", categoryId ?? (object)DBNull.Value },
                { "@MonthYear", monthYear }
            };

            return await Task.Run(() =>
            {
                var result = _dbHelper.ExecuteScalar(SqlQueryHelper.GetBudgetIdIfExists, parameters);
                return result != null && result != DBNull.Value ? (int?)Convert.ToInt32(result) : null;
            });
        }

        /// <summary>
        /// Get current spending
        /// </summary>
        public async Task<decimal> GetCurrentSpendingAsync(int userId, int? categoryId, string monthYear)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@CategoryId", categoryId ?? (object)DBNull.Value },
                { "@MonthYear", monthYear }
            };

            return await Task.Run(() => _dbHelper.GetSum(SqlQueryHelper.GetCurrentSpending, parameters));
        }

        /// <summary>
        /// Get all budgets for a user
        /// </summary>
        public async Task<List<Budget>> GetAllBudgetsByUserAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() =>
            {
                var budgets = new List<Budget>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetAllBudgetsByUser, parameters);
                while (reader.Read())
                {
                    budgets.Add(MapBudgetFromReader(reader));
                }
                return budgets;
            });
        }

        // Helper methods
        private Budget MapBudgetFromReader(Microsoft.Data.SqlClient.SqlDataReader reader)
        {
            return new Budget
            {
                BudgetId = reader.GetInt32(reader.GetOrdinal("BudgetId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                BudgetAmount = reader.GetDecimal(reader.GetOrdinal("BudgetAmount")),
                MonthYear = reader.GetString(reader.GetOrdinal("MonthYear")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                UserFullName = reader.GetString(reader.GetOrdinal("UserFullName"))
            };
        }

        private string GetBudgetStatus(decimal percentageUsed, bool isOverBudget)
        {
            if (isOverBudget) return "Exceeded";
            if (percentageUsed >= 90) return "Critical";
            if (percentageUsed >= 80) return "Warning";
            return "On Track";
        }

        private string GetAlertLevel(decimal percentageUsed)
        {
            if (percentageUsed >= 100) return "Alert";
            if (percentageUsed >= 90) return "Warning";
            if (percentageUsed >= 80) return "Info";
            return "None";
        }

        private string GetAlertMessage(decimal percentageUsed, decimal spending, decimal budget)
        {
            if (percentageUsed >= 100)
                return $"Budget exceeded! You've spent ₹{spending:N2} of ₹{budget:N2} ({percentageUsed:F1}%).";
            if (percentageUsed >= 90)
                return $"Warning: You've used {percentageUsed:F1}% of your budget. ₹{spending:N2} / ₹{budget:N2}.";
            if (percentageUsed >= 80)
                return $"Info: You've used {percentageUsed:F1}% of your budget. ₹{spending:N2} / ₹{budget:N2}.";
            return null!;
        }
    }
}