using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Notification Service Interface
    /// Defines business logic operations for notification management
    /// </summary>
    public interface INotificationService
    {
        // GET /api/notifications/my/{userId}
        Task<(bool success, string message, List<NotificationDTO>? data)> GetMyNotificationsAsync(int userId);

        // GET /api/notifications/unread-count/{userId}
        Task<(bool success, string message, int count)> GetUnreadNotificationCountAsync(int userId);

        // PUT /api/notifications/mark-read/{notificationId}
        Task<(bool success, string message)> MarkNotificationAsReadAsync(int notificationId, int requestingUserId);

        // DELETE /api/notifications/{notificationId}
        Task<(bool success, string message)> DeleteNotificationAsync(int notificationId, int requestingUserId);

        // Auto-triggered notifications
        Task SendBudgetAlertAsync(int userId, string categoryName, decimal percentageUsed, decimal budgetAmount, decimal currentSpending, string alertType);
        Task SendExpenseAddedNotificationAsync(int userId, decimal amount, int categoryId, int expenseId);
        Task SendMonthlySummaryNotificationAsync(int userId, string monthYear, decimal totalSpending, int expenseCount);
    }
}