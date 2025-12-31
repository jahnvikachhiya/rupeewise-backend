using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Notification Service Implementation
    /// Handles notification management and auto-triggered notifications
    /// Notification Types: Info (🔵), Warning (🟡), Alert (🔴), Success (🟢)
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICategoryRepository _categoryRepository;

        public NotificationService(INotificationRepository notificationRepository, ICategoryRepository categoryRepository)
        {
            _notificationRepository = notificationRepository;
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// GET /api/notifications/my/{userId} - Get user's notifications
        /// </summary>
        public async Task<(bool success, string message, List<NotificationDTO>? data)> GetMyNotificationsAsync(int userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
                var notificationDtos = notifications.Select(MapToNotificationDTO).ToList();
                return (true, "Notifications retrieved successfully", notificationDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/notifications/unread-count/{userId} - Get unread notification count
        /// </summary>
        public async Task<(bool success, string message, int count)> GetUnreadNotificationCountAsync(int userId)
        {
            try
            {
                int count = await _notificationRepository.GetUnreadNotificationCountAsync(userId);
                return (true, "Unread count retrieved successfully", count);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// PUT /api/notifications/mark-read/{notificationId} - Mark notification as read
        /// </summary>
        public async Task<(bool success, string message)> MarkNotificationAsReadAsync(int notificationId, int requestingUserId)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);

                if (notification == null)
                {
                    return (false, "Notification not found");
                }

                // Check access: Owner only
                if (notification.UserId != requestingUserId)
                {
                    return (false, "You don't have permission to mark this notification as read");
                }

                bool marked = await _notificationRepository.MarkNotificationAsReadAsync(notificationId);

                if (marked)
                {
                    return (true, "Notification marked as read");
                }

                return (false, "Failed to mark notification as read");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE /api/notifications/{notificationId} - Delete notification
        /// </summary>
        public async Task<(bool success, string message)> DeleteNotificationAsync(int notificationId, int requestingUserId)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);

                if (notification == null)
                {
                    return (false, "Notification not found");
                }

                // Check access: Owner only
                if (notification.UserId != requestingUserId)
                {
                    return (false, "You don't have permission to delete this notification");
                }

                bool deleted = await _notificationRepository.DeleteNotificationAsync(notificationId);

                if (deleted)
                {
                    return (true, "Notification deleted successfully");
                }

                return (false, "Failed to delete notification");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Send budget alert notification (80%, 90%, 100%)
        /// Auto-triggered by expense creation/update
        /// </summary>
        public async Task SendBudgetAlertAsync(int userId, string categoryName, decimal percentageUsed, decimal budgetAmount, decimal currentSpending, string alertType)
        {
            try
            {
                string title = "";
                string message = "";
                string type = "";

                switch (alertType)
                {
                    case "Info": // 80% threshold
                        title = $"🔵 Budget Alert: {categoryName}";
                        message = $"You've used {percentageUsed:F1}% of your {categoryName} budget. Current spending: ₹{currentSpending:N2} / ₹{budgetAmount:N2}";
                        type = "Info";
                        break;

                    case "Warning": // 90% threshold
                        title = $"🟡 Budget Warning: {categoryName}";
                        message = $"You've used {percentageUsed:F1}% of your {categoryName} budget. Current spending: ₹{currentSpending:N2} / ₹{budgetAmount:N2}. Consider reducing expenses.";
                        type = "Warning";
                        break;

                    case "Alert": // 100% threshold
                        title = $"🔴 Budget Exceeded: {categoryName}";
                        message = $"You've exceeded your {categoryName} budget! Current spending: ₹{currentSpending:N2} / ₹{budgetAmount:N2} ({percentageUsed:F1}%). Please review your expenses.";
                        type = "Alert";
                        break;
                }

                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.CreateNotificationAsync(notification);
            }
            catch
            {
                // Silently fail - don't block expense operations if notification fails
            }
        }

        /// <summary>
        /// Send expense added confirmation notification
        /// Auto-triggered by expense creation
        /// </summary>
        public async Task SendExpenseAddedNotificationAsync(int userId, decimal amount, int categoryId, int expenseId)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
                string categoryName = category?.CategoryName ?? "Unknown";

                var notification = new Notification
                {
                    UserId = userId,
                    Title = "🟢 Expense Added Successfully",
                    Message = $"Your expense of ₹{amount:N2} in {categoryName} category has been recorded.",
                    Type = "Success",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.CreateNotificationAsync(notification);
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Send monthly summary notification
        /// Auto-triggered at end of month (to be implemented with scheduled job)
        /// </summary>
        public async Task SendMonthlySummaryNotificationAsync(int userId, string monthYear, decimal totalSpending, int expenseCount)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = $"📊 Monthly Summary - {monthYear}",
                    Message = $"You spent ₹{totalSpending:N2} across {expenseCount} transactions this month. View detailed breakdown in reports.",
                    Type = "Info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.CreateNotificationAsync(notification);
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Helper: Map Notification to NotificationDTO
        /// </summary>
        private NotificationDTO MapToNotificationDTO(Notification notification)
        {
            return new NotificationDTO
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                UserFullName = notification.UserFullName ?? string.Empty
            };
        }
    }
}