using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Repositories;
using System.Security.Claims;
using System.Text;
using ExpenseManagementAPI.Services;

   namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPdfReportService _pdfReportService; // ✅ Changed to interface

        public ReportsController(
            DatabaseHelper dbHelper,
            IExpenseRepository expenseRepository,
            ICategoryRepository categoryRepository,
            IPdfReportService pdfReportService)   // ✅ Changed to interface
        {
            _dbHelper = dbHelper;
            _expenseRepository = expenseRepository;
            _categoryRepository = categoryRepository;
            _pdfReportService = pdfReportService;
        }

        // ... rest of your controller code
    


        /// <summary>
        /// GET /api/reports/monthly/{userId} - Generate monthly report
        /// Access: Authenticated User
        /// Query params: month, year
        /// </summary>
        [HttpGet("monthly/{userId}")]
        public async Task<IActionResult> GetMonthlyReport(int userId, [FromQuery] int month, [FromQuery] int year)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own reports
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@Month", month },
                    { "@Year", year }
                };
                    MonthlyReportDTO? report = null;

                    // Get monthly summary
                    using (var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetMonthlyReport, parameters))
                    {
                        if (reader.Read())
                        {
                            report = new MonthlyReportDTO
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                UserFullName = reader.GetString(reader.GetOrdinal("UserFullName")),
                                Month = reader.GetInt32(reader.GetOrdinal("Month")),
                                Year = reader.GetInt32(reader.GetOrdinal("Year")),
                                MonthYear = reader.GetString(reader.GetOrdinal("MonthYear")),
                                TotalExpenses = reader.GetDecimal(reader.GetOrdinal("TotalExpenses")),
                                TotalExpenseCount = reader.GetInt32(reader.GetOrdinal("TotalExpenseCount"))
                            };
                        }
                    }

                    if (report == null)
                    {
                        return NotFound(ApiResponse<object>.ErrorResponse("No data found for specified month", 404));
                    }

                    // Get category breakdown
                    var categoryParams = new Dictionary<string, object>
                    {
                        { "@UserId", userId },
                        { "@StartDate", new DateTime(year, month, 1) },
                        { "@EndDate", new DateTime(year, month, DateTime.DaysInMonth(year, month)) }
                    };

                    using (var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetCategoryBreakdown, categoryParams))
                    {
                        report.CategoryBreakdown = new List<CategoryBreakdownDTO>();
                        while (reader.Read())
                        {
                            report.CategoryBreakdown.Add(new CategoryBreakdownDTO
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
                    }

                    // Get top 10 expenses
                    var topExpensesParams = new Dictionary<string, object>
                    {
                        { "@UserId", userId },
                        { "@Month", month },
                        { "@Year", year },
                        { "@TopN", 10 }
                    };

                    using (var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetTopExpensesForMonth, topExpensesParams))
                    {
                        report.TopExpenses = new List<ExpenseDTO>();
                        while (reader.Read())
                        {
                            report.TopExpenses.Add(new ExpenseDTO
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
                                UserFullName = reader.GetString(reader.GetOrdinal("UserFullName"))
                            });
                        }
                    }

                    return Ok(ApiResponse<MonthlyReportDTO>.SuccessResponse(report, "Monthly report generated successfully"));
        
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Error: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// GET /api/reports/export-pdf - Export monthly report to PDF
        /// Access: Authenticated User
        /// Query params: month, year
        /// </summary>
       
        [Authorize]
[HttpGet("export-pdf")]
public async Task<IActionResult> ExportMonthlyReportPdf([FromQuery] int month, [FromQuery] int year)
{
    var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // Get report data (reuse existing logic from GetMonthlyReport)
    var parameters = new Dictionary<string, object>
    {
        { "@UserId", requestingUserId },
        { "@Month", month },
        { "@Year", year }
    };

    MonthlyReportDTO? report = null;

    using (var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetMonthlyReport, parameters))
    {
        if (reader.Read())
        {
            report = new MonthlyReportDTO
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserFullName = reader.GetString(reader.GetOrdinal("UserFullName")),
                Month = reader.GetInt32(reader.GetOrdinal("Month")),
                Year = reader.GetInt32(reader.GetOrdinal("Year")),
                MonthYear = reader.GetString(reader.GetOrdinal("MonthYear")),
                TotalExpenses = reader.GetDecimal(reader.GetOrdinal("TotalExpenses")),
                TotalExpenseCount = reader.GetInt32(reader.GetOrdinal("TotalExpenseCount"))
            };
        }
    }

    if (report == null)
        return NotFound(ApiResponse<object>.ErrorResponse("No data found for this month", 404));

    // Category breakdown
    var categoryParams = new Dictionary<string, object>
    {
        { "@UserId", requestingUserId },
        { "@StartDate", new DateTime(year, month, 1) },
        { "@EndDate", new DateTime(year, month, DateTime.DaysInMonth(year, month)) }
    };

    using (var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetCategoryBreakdown, categoryParams))
    {
        report.CategoryBreakdown = new List<CategoryBreakdownDTO>();
        while (reader.Read())
        {
            report.CategoryBreakdown.Add(new CategoryBreakdownDTO
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                ExpenseCount = reader.GetInt32(reader.GetOrdinal("ExpenseCount")),
                PercentageOfTotal = reader.GetDecimal(reader.GetOrdinal("PercentageOfTotal"))
            });
        }
    }

    // Top 10 expenses
    var topExpensesParams = new Dictionary<string, object>
    {
        { "@UserId", requestingUserId },
        { "@Month", month },
        { "@Year", year },
        { "@TopN", 10 }
    };

    using (var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetTopExpensesForMonth, topExpensesParams))
    {
        report.TopExpenses = new List<ExpenseDTO>();
        while (reader.Read())
        {
            report.TopExpenses.Add(new ExpenseDTO
            {
                ExpenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            });
        }
    }

    // Generate PDF
    var pdfBytes = _pdfReportService.GenerateMonthlyReportPdf(report);
    var fileName = $"ExpenseReport_{report.MonthYear}.pdf";

    return File(pdfBytes, "application/pdf", fileName);
}

        /// <summary>
        /// GET /api/reports/export-csv/{userId} - Export expenses to CSV
        /// Access: Authenticated User
        /// Query params: startDate, endDate
                    /// </summary>
            [Authorize(Roles = "Admin")]
            [HttpGet("export-csv")]
            public async Task<IActionResult> ExportToCSV(
                [FromQuery] DateTime? startDate,
                [FromQuery] DateTime? endDate,
                [FromQuery] int? userId
            )
            {

            try
            {
                // Default to current month if dates not provided
                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? DateTime.Now;

                var expenses = await _expenseRepository.FilterExpensesAsync(
                userId , null, start, end, null, null, null);

                // Generate CSV
                var csv = new StringBuilder();
               csv.AppendLine("Expense ID,Date,Category,Amount,Description,Payment Method,Status");

                foreach (var expense in expenses)
                {
                    csv.AppendLine(
                    $"{expense.ExpenseId}," +
                    $"{expense.ExpenseDate:yyyy-MM-dd}," +
                    $"{expense.CategoryName}," +
                    $"{expense.Amount:F2}," +
                    $"\"{expense.Description}\"," +
                    $"{expense.PaymentMethod}," +
                    $"{expense.Status}"
                );

                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"Expenses_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Error: {ex.Message}", 400));
            }
        }
    }
}