using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for Expense
    /// Used in GET /api/expenses/my/{userId}, GET /api/expenses/{expenseId}, GET /api/expenses/filter, GET /api/expenses/search
    /// </summary>
    public class ExpenseDTO
    {
        public int ExpenseId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Approved";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties from JOIN queries
        public string CategoryName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for POST /api/expenses (Create new expense)
    /// </summary>
    public class CreateExpenseDTO
    {
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Expense date is required")]
        public DateTime ExpenseDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "Cash";
    }

    /// <summary>
    /// Request DTO for PUT /api/expenses/{expenseId} (Update expense)
    /// </summary>
    public class UpdateExpenseDTO
    {
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Expense date is required")]
        public DateTime ExpenseDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "Cash";
    }
}