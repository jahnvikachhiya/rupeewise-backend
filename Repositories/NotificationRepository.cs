using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// Notification Repository Implementation
    /// Handles all notification-related database operations using ADO.NET
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public NotificationRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Create notification (auto-triggered by system)
        /// </summary>
        public async Task<int> CreateNotificationAsync(Notification notification)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", notification.UserId },
                { "@Title", notification.Title },
                { "@Message", notification.Message },
                { "@Type", notification.Type }
            };

            return await Task.Run(() => _dbHelper.ExecuteInsertWithId(SqlQueryHelper.InsertNotification, parameters));
        }

        /// <summary>
        /// GET /api/notifications/my/{userId} - Get user's notifications
        /// </summary>
        public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() =>
            {
                var notifications = new List<Notification>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetNotificationsByUserId, parameters);
                while (reader.Read())
                {
                    notifications.Add(MapNotificationFromReader(reader));
                }
                return notifications;
            });
        }

        /// <summary>
        /// GET /api/notifications/unread-count/{userId} - Get unread count
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.GetCount(SqlQueryHelper.GetUnreadNotificationCount, parameters));
        }

        /// <summary>
        /// PUT /api/notifications/mark-read/{notificationId} - Mark as read
        /// </summary>
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@NotificationId", notificationId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.MarkNotificationAsRead, parameters) > 0);
        }

        /// <summary>
        /// DELETE /api/notifications/{notificationId} - Delete notification
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@NotificationId", notificationId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.DeleteNotification, parameters) > 0);
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@NotificationId", notificationId }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetNotificationById, parameters);
                if (reader.Read())
                {
                    return MapNotificationFromReader(reader);
                }
                return null;
            });
        }

        /// <summary>
        /// Helper method to map SqlDataReader to Notification object
        /// </summary>
        private Notification MapNotificationFromReader(Microsoft.Data.SqlClient.SqlDataReader reader)
        {
            return new Notification
            {
                NotificationId = reader.GetInt32(reader.GetOrdinal("NotificationId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Message = reader.GetString(reader.GetOrdinal("Message")),
                Type = reader.GetString(reader.GetOrdinal("Type")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                ReadAt = reader.IsDBNull(reader.GetOrdinal("ReadAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ReadAt")),
                UserFullName = reader.GetString(reader.GetOrdinal("UserFullName"))
            };
        }
    }
}