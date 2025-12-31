using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Services;
using System.Security.Claims;

namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    [Authorize]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetsController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        /// <summary>
        /// POST /api/budgets - Create/Set budget (overall or category-specific)
        /// Access: Authenticated User
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message, data) = await _budgetService.CreateBudgetAsync(userId, request);

            if (success)
            {
                return Ok(ApiResponse<BudgetDTO>.SuccessResponse(data!, message, 201));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/budgets/my/{userId}?monthYear={monthYear} - Get user's budgets for a month
        /// Access: Authenticated User (Own Budgets Only)
        /// </summary>
        [HttpGet("my/{userId}")]
        public async Task<IActionResult> GetMyBudgets(int userId, [FromQuery] string monthYear)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own budgets
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(monthYear))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("MonthYear query parameter is required", 400));
            }

            var (success, message, data) = await _budgetService.GetMyBudgetsAsync(userId, monthYear);

            if (success)
            {
                return Ok(ApiResponse<List<BudgetDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/budgets/{budgetId} - Get budget by ID
        /// Access: Authenticated User (Owner) OR Admin
        /// </summary>
        [HttpGet("{budgetId}")]
        public async Task<IActionResult> GetBudgetById(int budgetId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var (success, message, data) = await _budgetService.GetBudgetByIdAsync(budgetId, requestingUserId, userRole);

            if (success)
            {
                return Ok(ApiResponse<BudgetDTO>.SuccessResponse(data!, message));
            }

            return NotFound(ApiResponse<object>.ErrorResponse(message, 404));
        }

        /// <summary>
        /// PUT /api/budgets/{budgetId} - Update budget
        /// Access: Authenticated User (Owner) OR Admin
        /// </summary>
        [HttpPut("{budgetId}")]
        public async Task<IActionResult> UpdateBudget(int budgetId, [FromBody] UpdateBudgetDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var (success, message) = await _budgetService.UpdateBudgetAsync(budgetId, requestingUserId, userRole, request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// DELETE /api/budgets/{budgetId} - Delete budget
        /// Access: Authenticated User (Owner) OR Admin
        /// </summary>
        [HttpDelete("{budgetId}")]
        public async Task<IActionResult> DeleteBudget(int budgetId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var (success, message) = await _budgetService.DeleteBudgetAsync(budgetId, requestingUserId, userRole);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/budgets/vs-actual/{userId} - Budget vs Actual spending comparison
        /// Access: Authenticated User
        /// Query params: categoryId (optional), monthYear (required)
        /// </summary>
        [HttpGet("vs-actual/{userId}")]
        public async Task<IActionResult> GetBudgetVsActual(
            int userId, 
            [FromQuery] int? categoryId, 
            [FromQuery] string monthYear)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own budget comparison
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(monthYear))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("MonthYear query parameter is required", 400));
            }

            var (success, message, data) = await _budgetService.GetBudgetVsActualAsync(userId, categoryId, monthYear);

            if (success)
            {
                return Ok(ApiResponse<BudgetVsActualDTO>.SuccessResponse(data!, message));
            }

            return NotFound(ApiResponse<object>.ErrorResponse(message, 404));
        }

        /// <summary>
        /// GET /api/budgets/status/{userId} - Check budget status and percentage used
        /// Access: Authenticated User
        /// Query params: categoryId (optional), monthYear (required)
        /// Triggers alerts at 80% (Info), 90% (Warning), 100% (Alert)
        /// </summary>
        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetBudgetStatus(
            int userId, 
            [FromQuery] int? categoryId, 
            [FromQuery] string monthYear)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only check their own budget status
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(monthYear))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("MonthYear query parameter is required", 400));
            }

            var (success, message, data) = await _budgetService.GetBudgetStatusAsync(userId, categoryId, monthYear);

            if (success)
            {
                return Ok(ApiResponse<BudgetStatusDTO>.SuccessResponse(data!, message));
            }

            return NotFound(ApiResponse<object>.ErrorResponse(message, 404));
        }
    }
}