namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Expense entity - Maps to Expenses table in database
    /// Table Columns: ExpenseId, UserId, CategoryId, Amount, ExpenseDate, Description, PaymentMethod, Status, CreatedAt, UpdatedAt
    /// Foreign Keys: UserId → Users.UserId, CategoryId → Categories.CategoryId
    /// </summary>
    public class Expense
    {
        /// <summary>
        /// ExpenseId (INT, Primary Key, Identity)
        /// </summary>
        public int ExpenseId { get; set; }

        /// <summary>
        /// UserId (INT, NOT NULL, Foreign Key → Users.UserId)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// CategoryId (INT, NOT NULL, Foreign Key → Categories.CategoryId)
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Amount (DECIMAL(18,2), NOT NULL)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ExpenseDate (DATE, NOT NULL)
        /// </summary>
        public DateTime ExpenseDate { get; set; }

        /// <summary>
        /// Description (NVARCHAR(500))
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// PaymentMethod (NVARCHAR(50))
        /// Values: 'Cash', 'Card', 'UPI', 'Net Banking', 'Others'
        /// </summary>
        public string PaymentMethod { get; set; } = "Cash";

        /// <summary>
        /// Status (NVARCHAR(50), DEFAULT 'Approved')
        /// Values: 'Pending', 'Approved', 'Rejected'
        /// </summary>
        public string Status { get; set; } = "Approved";

        /// <summary>
        /// CreatedAt (DATETIME, DEFAULT GETDATE())
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UpdatedAt (DATETIME, DEFAULT GETDATE())
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (loaded from JOIN queries, not database columns)
        public string? CategoryName { get; set; }
        public string? UserFullName { get; set; }
         public string? Username { get; set; }
    }
}