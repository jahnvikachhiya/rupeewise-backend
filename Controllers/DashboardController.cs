using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Data;
using System.Security.Claims;

namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public DashboardController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// GET /api/dashboard/summary/{userId} - Dashboard summary (total, monthly, today expenses)
        /// Access: Authenticated User
        /// </summary>
        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetDashboardSummary(int userId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own dashboard
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            try
            {
                var parameters = new Dictionary<string, object> { { "@UserId", userId } };

                    using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetDashboardSummary, parameters);
                    if (reader.Read())
                    {
                        var summary = new DashboardSummaryDTO
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            UserFullName = reader.GetString(reader.GetOrdinal("UserFullName")),
                            TotalExpenses = reader.GetDecimal(reader.GetOrdinal("TotalExpenses")),
                            TotalExpenseCount = reader.GetInt32(reader.GetOrdinal("TotalExpenseCount")),
                            MonthlyExpenses = reader.GetDecimal(reader.GetOrdinal("MonthlyExpenses")),
                            MonthlyExpenseCount = reader.GetInt32(reader.GetOrdinal("MonthlyExpenseCount")),
                            TodayExpenses = reader.GetDecimal(reader.GetOrdinal("TodayExpenses")),
                            TodayExpenseCount = reader.GetInt32(reader.GetOrdinal("TodayExpenseCount"))
                        };

                        return Ok(ApiResponse<DashboardSummaryDTO>.SuccessResponse(summary, "Dashboard summary retrieved successfully"));
                    }

                    return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));
                
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Error: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// GET /api/dashboard/category-breakdown/{userId} - Category-wise breakdown
        /// Access: Authenticated User
        /// Query params: startDate, endDate
        /// </summary>
        [HttpGet("category-breakdown/{userId}")]
        public async Task<IActionResult> GetCategoryBreakdown(
            int userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own breakdown
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            try
            {
                // Default to current month if dates not provided
                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? DateTime.Now;

                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@StartDate", start },
                    { "@EndDate", end }
                };

                return await Task.Run(() =>
                {
                    var breakdown = new List<CategoryBreakdownDTO>();
                    using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetCategoryBreakdown, parameters);
                    while (reader.Read())
                    {
                        breakdown.Add(new CategoryBreakdownDTO
                        {
                            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                            Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString(reader.GetOrdinal("Icon")),
                            ColorCode = reader.IsDBNull(reader.GetOrdinal("ColorCode")) ? null : reader.GetString(reader.GetOrdinal("ColorCode")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            ExpenseCount = reader.GetInt32(reader.GetOrdinal("ExpenseCount")),
                            PercentageOfTotal = reader.GetDecimal(reader.GetOrdinal("PercentageOfTotal"))
                        });
                    }

                    return Ok(ApiResponse<List<CategoryBreakdownDTO>>.SuccessResponse(breakdown, "Category breakdown retrieved successfully"));
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Error: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// GET /api/dashboard/monthly-trend/{userId} - Monthly spending trend
        /// Access: Authenticated User
        /// Query params: year
        /// </summary>
        [HttpGet("monthly-trend/{userId}")]
        public async Task<IActionResult> GetMonthlyTrend(int userId, [FromQuery] int? year)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own trend
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            try
            {
                var targetYear = year ?? DateTime.Now.Year;
                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@Year", targetYear }
                };

                return await Task.Run(() =>
                {
                    var trend = new List<MonthlyTrendDTO>();
                    using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetMonthlyTrend, parameters);
                    while (reader.Read())
                    {
                        trend.Add(new MonthlyTrendDTO
                        {
                            MonthYear = reader.GetString(reader.GetOrdinal("MonthYear")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            ExpenseCount = reader.GetInt32(reader.GetOrdinal("ExpenseCount"))
                        });
                    }

                    return Ok(ApiResponse<List<MonthlyTrendDTO>>.SuccessResponse(trend, "Monthly trend retrieved successfully"));
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Error: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// GET /api/dashboard/budget-progress/{userId} - Budget progress for all categories
        /// Access: Authenticated User
        /// Query params: monthYear
        /// </summary>
        [HttpGet("budget-progress/{userId}")]
        public async Task<IActionResult> GetBudgetProgress(int userId, [FromQuery] string? monthYear)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own budget progress
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            try
            {
                var targetMonthYear = monthYear ?? DateTime.Now.ToString("yyyy-MM");
                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@MonthYear", targetMonthYear }
                };

                return await Task.Run(() =>
                {
                    var progress = new List<BudgetProgressDTO>();
                    using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetBudgetProgress, parameters);
                    while (reader.Read())
                    {
                        var budgetAmount = reader.GetDecimal(reader.GetOrdinal("BudgetAmount"));
                        var currentSpending = reader.GetDecimal(reader.GetOrdinal("CurrentSpending"));
                        var remaining = budgetAmount - currentSpending;
                        var percentageUsed = budgetAmount > 0 ? (currentSpending / budgetAmount) * 100 : 0;

                        string status = "On Track";
                        if (currentSpending > budgetAmount) status = "Exceeded";
                        else if (percentageUsed >= 90) status = "Critical";
                        else if (percentageUsed >= 80) status = "Warning";

                        progress.Add(new BudgetProgressDTO
                        {
                            BudgetId = reader.IsDBNull(reader.GetOrdinal("BudgetId")) ? null : reader.GetInt32(reader.GetOrdinal("BudgetId")),
                            CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                            CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? "Overall" : reader.GetString(reader.GetOrdinal("CategoryName")),
                            BudgetAmount = budgetAmount,
                            CurrentSpending = currentSpending,
                            RemainingBudget = remaining,
                            PercentageUsed = percentageUsed,
                            BudgetStatus = status
                        });
                    }

                    return Ok(ApiResponse<List<BudgetProgressDTO>>.SuccessResponse(progress, "Budget progress retrieved successfully"));
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Error: {ex.Message}", 400));
            }
        }
    }
}