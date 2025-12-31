namespace ExpenseManagementAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for Notification
    /// Used in GET /api/notifications/my/{userId}
    /// Notification Types: Info (🔵), Warning (🟡), Alert (🔴), Success (🟢)
    /// </summary>
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }

        // Navigation property
        public string UserFullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for GET /api/notifications/unread-count/{userId}
    /// </summary>
    public class UnreadCountDTO
    {
        public int UserId { get; set; }
        public int UnreadCount { get; set; }
    }
}