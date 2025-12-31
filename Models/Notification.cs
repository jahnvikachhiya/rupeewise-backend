namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Notification entity - Maps to Notifications table in database
    /// Table Columns: NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, ReadAt
    /// Foreign Key: UserId → Users.UserId
    /// Notification Types: Info (🔵), Warning (🟡), Alert (🔴), Success (🟢)
    /// Auto-triggered for: Budget alerts (80%, 90%, 100%), Expense confirmations, Monthly summaries
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// NotificationId (INT, Primary Key, Identity)
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        /// UserId (INT, NOT NULL, Foreign Key → Users.UserId)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Title (NVARCHAR(200), NOT NULL)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Message (NVARCHAR(1000), NOT NULL)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Type (NVARCHAR(50), NOT NULL)
        /// Values: 'Info', 'Warning', 'Alert', 'Success'
        /// </summary>
        public string Type { get; set; } = "Info";

        /// <summary>
        /// IsRead (BIT, DEFAULT 0)
        /// 0 = Unread, 1 = Read
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// CreatedAt (DATETIME, DEFAULT GETDATE())
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ReadAt (DATETIME, NULL)
        /// </summary>
        public DateTime? ReadAt { get; set; }

        // Navigation property (loaded from JOIN queries)
        public string? UserFullName { get; set; }
    }
}