using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Services;
using System.Security.Claims;

namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IBudgetService _budgetService;

        public NotificationsController(INotificationService notificationService, IBudgetService budgetService)
        {
            _notificationService = notificationService;
            _budgetService = budgetService;
        }

        /// <summary>
        /// GET /api/notifications/my/{userId} - Get user's notifications
        /// Access: Authenticated User (Own Notifications)
        /// </summary>
        [HttpGet("my/{userId}")]
        public async Task<IActionResult> GetMyNotifications(int userId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only view their own notifications
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message, data) = await _notificationService.GetMyNotificationsAsync(userId);

            if (success)
            {
                return Ok(ApiResponse<List<NotificationDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/notifications/unread-count/{userId} - Get unread notification count
        /// Access: Authenticated User (Own Data)
        /// </summary>
        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only get their own unread count
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message, count) = await _notificationService.GetUnreadNotificationCountAsync(userId);

            if (success)
            {
                return Ok(ApiResponse<UnreadCountDTO>.SuccessResponse(
                    new UnreadCountDTO { UserId = userId, UnreadCount = count }, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// PUT /api/notifications/mark-read/{notificationId} - Mark notification as read
        /// Access: Authenticated User (Owner)
        /// </summary>
        [HttpPut("mark-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message) = await _notificationService.MarkNotificationAsReadAsync(notificationId, requestingUserId);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// DELETE /api/notifications/{notificationId} - Delete notification
        /// Access: Authenticated User (Owner)
        /// </summary>
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message) = await _notificationService.DeleteNotificationAsync(notificationId, requestingUserId);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// POST /api/notifications/send-budget-alert - Manually trigger budget alert check
        /// Access: System/Admin (Can be triggered manually for testing)
        /// Query params: userId, categoryId (optional), monthYear
        /// </summary>
        [HttpPost("send-budget-alert")]
        public async Task<IActionResult> SendBudgetAlert(
            [FromQuery] int userId, 
            [FromQuery] int? categoryId, 
            [FromQuery] string monthYear)
        {
            if (string.IsNullOrEmpty(monthYear))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("MonthYear query parameter is required", 400));
            }

            await _budgetService.CheckBudgetAndSendAlertsAsync(userId, categoryId, monthYear);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Budget alert check triggered successfully"));
        }
    }
}