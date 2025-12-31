using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Services;
using System.Security.Claims;

namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// GET /api/categories/my/{userId} - Get system categories + user's custom categories
        /// Access: Authenticated User (own categories) OR Admin (all categories)
        /// </summary>
        [HttpGet("my/{userId}")]
        public async Task<IActionResult> GetMyCategories(int userId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            // Admin can view all categories, users can only view their own
            if (userRole != "Admin" && requestingUserId != userId)
            {
                return Forbid();
            }

            bool success;
            string message;
            List<CategoryDTO>? data;

            // If admin, get all categories (system + all custom from all users)
            // If user, get only their categories (system + their custom)
            if (userRole == "Admin")
            {
                (success, message, data) = await _categoryService.GetAllCategoriesAsync();
            }
            else
            {
                (success, message, data) = await _categoryService.GetMyCategoriesAsync(userId);
            }

            if (success)
            {
                return Ok(ApiResponse<List<CategoryDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// POST /api/categories/custom - Create custom category
        /// Access: Authenticated User
        /// </summary>
        [HttpPost("custom")]
        public async Task<IActionResult> CreateCustomCategory([FromBody] CreateCategoryDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message, data) = await _categoryService.CreateCustomCategoryAsync(userId, request);

            if (success)
            {
                return Ok(ApiResponse<CategoryDTO>.SuccessResponse(data!, message, 201));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// POST /api/categories/system - Create system category
        /// Access: Admin Only
        /// </summary>
        [HttpPost("system")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSystemCategory([FromBody] CreateCategoryDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var (success, message, data) = await _categoryService.CreateSystemCategoryAsync(request);

            if (success)
            {
                return Ok(ApiResponse<CategoryDTO>.SuccessResponse(data!, message, 201));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// PUT /api/categories/custom/{categoryId} - Update custom category
        /// Access: Authenticated User (Owner)
        /// </summary>
        [HttpPut("custom/{categoryId}")]
        public async Task<IActionResult> UpdateCustomCategory(int categoryId, [FromBody] UpdateCategoryDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message) = await _categoryService.UpdateCustomCategoryAsync(categoryId, userId, request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// PUT /api/categories/system/{categoryId} - Update system category
        /// Access: Admin Only
        /// </summary>
        [HttpPut("system/{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSystemCategory(int categoryId, [FromBody] UpdateCategoryDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var (success, message) = await _categoryService.UpdateSystemCategoryAsync(categoryId, request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// DELETE /api/categories/custom/{categoryId} - Delete custom category
        /// Access: Authenticated User (Owner)
        /// </summary>
        [HttpDelete("custom/{categoryId}")]
        public async Task<IActionResult> DeleteCustomCategory(int categoryId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message) = await _categoryService.DeleteCustomCategoryAsync(categoryId, userId);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// DELETE /api/categories/system/{categoryId} - Delete system category
        /// Access: Admin Only
        /// </summary>
        [HttpDelete("system/{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSystemCategory(int categoryId)
        {
            var (success, message) = await _categoryService.DeleteSystemCategoryAsync(categoryId);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/categories/statistics/{userId} - Category-wise spending statistics
        /// Access: Authenticated User
        /// </summary>
        [HttpGet("statistics/{userId}")]
        public async Task<IActionResult> GetCategoryStatistics(int userId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own statistics
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message, data) = await _categoryService.GetCategoryStatisticsAsync(userId);

            if (success)
            {
                return Ok(ApiResponse<List<CategoryStatisticsDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }
    }
}