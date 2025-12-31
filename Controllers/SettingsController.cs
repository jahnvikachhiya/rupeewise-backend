using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Services;
using System.Security.Claims;

namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly IUserSettingsService _settingsService;

        public SettingsController(IUserSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var (success, message, data) = await _settingsService.GetUserSettingsAsync(userId);

            if (success)
            {
                return Ok(ApiResponse<UserSettingsDTO>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateUserSettingsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var (success, message) = await _settingsService.UpdateUserSettingsAsync(userId, request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }
    }
}