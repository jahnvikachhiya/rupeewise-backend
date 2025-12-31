using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Services;
using System.Security.Claims;


namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        /// <summary>
/// POST /api/expenses - Create new expense
/// </summary>
[HttpPost]
public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDTO request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
    }

    // ✅ Get userId from JWT token, not from request body
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    
    var (success, message, data) = await _expenseService.CreateExpenseAsync(userId, request);

    if (success)
    {
        return Ok(ApiResponse<ExpenseDTO>.SuccessResponse(data!, message, 201));
    }

    return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
}
        /// <summary>
        /// GET /api/expenses/my/{userId} - Get user's own expenses
        /// Access: Authenticated User (Own Expenses Only)
        /// </summary>
        [HttpGet("my/{userId}")]
        public async Task<IActionResult> GetMyExpenses(int userId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own expenses
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message, data) = await _expenseService.GetMyExpensesAsync(userId);

            if (success)
            {
                return Ok(ApiResponse<List<ExpenseDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/expenses/{expenseId} - Get expense by ID
        /// Access: Authenticated User (Owner) OR Admin
        /// </summary>
        [HttpGet("{expenseId}")]
        public async Task<IActionResult> GetExpenseById(int expenseId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var (success, message, data) = await _expenseService.GetExpenseByIdAsync(expenseId, requestingUserId, userRole);

            if (success)
            {
                return Ok(ApiResponse<ExpenseDTO>.SuccessResponse(data!, message));
            }

            return NotFound(ApiResponse<object>.ErrorResponse(message, 404));
        }

        /// <summary>
        /// PUT /api/expenses/{expenseId} - Update expense
        /// Access: Authenticated User (Owner) OR Admin
        /// </summary>
        [HttpPut("{expenseId}")]
        public async Task<IActionResult> UpdateExpense(int expenseId, [FromBody] UpdateExpenseDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var (success, message) = await _expenseService.UpdateExpenseAsync(expenseId, requestingUserId, userRole, request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// DELETE /api/expenses/{expenseId} - Delete expense
        /// Access: Authenticated User (Owner) OR Admin
        /// </summary>
        [HttpDelete("{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var (success, message) = await _expenseService.DeleteExpenseAsync(expenseId, requestingUserId, userRole);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/expenses/filter - Filter expenses by date, category, amount, payment method
        /// Access: Authenticated User (Own Expenses Only)
        /// Query params: userId, categoryId, startDate, endDate, minAmount, maxAmount, paymentMethod
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> FilterExpenses(
            [FromQuery] int userId,
            [FromQuery] int? categoryId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] decimal? minAmount,
            [FromQuery] decimal? maxAmount,
            [FromQuery] string? paymentMethod)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only filter their own expenses
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message, data) = await _expenseService.FilterExpensesAsync(
                userId, categoryId, startDate, endDate, minAmount, maxAmount, paymentMethod);

            if (success)
            {
                return Ok(ApiResponse<List<ExpenseDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/expenses/search - Search expenses by description or amount
        /// Access: Authenticated User (Own Expenses Only)
        /// Query params: userId, searchTerm
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchExpenses([FromQuery] int userId, [FromQuery] string searchTerm)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only search their own expenses
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message, data) = await _expenseService.SearchExpensesAsync(userId, searchTerm);

            if (success)
            {
                return Ok(ApiResponse<List<ExpenseDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/expenses/all - Get all expenses (Admin only)
        /// Access: Admin Only
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllExpenses()
        {
            var (success, message, data) = await _expenseService.GetAllExpensesAsync();

            if (success)
            {
                return Ok(ApiResponse<List<ExpenseDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }
    }
}