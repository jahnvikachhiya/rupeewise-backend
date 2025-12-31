namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// User entity - Maps to Users table in database
    /// Table Columns: UserId, Username, Email, PasswordHash, FullName, PhoneNumber, Role, CreatedAt, IsActive
    /// </summary>
    public class User
    {
        /// <summary>
        /// UserId (INT, Primary Key, Identity)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username (NVARCHAR(100), NOT NULL, UNIQUE)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email (NVARCHAR(100), NOT NULL, UNIQUE)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// PasswordHash (NVARCHAR(255), NOT NULL)
        /// Hashed using BCrypt - Never store plain passwords
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// FullName (NVARCHAR(200))
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// PhoneNumber (NVARCHAR(20))
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Role (NVARCHAR(50), DEFAULT 'User')
        /// Values: 'User' or 'Admin'
        /// Admin assigned based on AdminEmails in appsettings.json
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// CreatedAt (DATETIME, DEFAULT GETDATE())
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IsActive (BIT, DEFAULT 1)
        /// 1 = Active, 0 = Deactivated
        /// </summary>
        public bool IsActive { get; set; } = true;


        
    }
}