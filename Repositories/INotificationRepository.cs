using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// Notification Repository Interface
    /// Defines all notification-related database operations
    /// </summary>
    public interface INotificationRepository
    {
        // POST notification (auto-triggered)
        Task<int> CreateNotificationAsync(Notification notification);

        // GET /api/notifications/my/{userId}
        Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);

        // GET /api/notifications/unread-count/{userId}
        Task<int> GetUnreadNotificationCountAsync(int userId);

        // PUT /api/notifications/mark-read/{notificationId}
        Task<bool> MarkNotificationAsReadAsync(int notificationId);

        // DELETE /api/notifications/{notificationId}
        Task<bool> DeleteNotificationAsync(int notificationId);

        // Get notification by ID
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
    }
}