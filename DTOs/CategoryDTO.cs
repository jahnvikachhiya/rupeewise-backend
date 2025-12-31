using System.ComponentModel.DataAnnotations;

namespace ExpenseManagementAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for Category
    /// Used in GET /api/categories/my/{userId} (Returns system categories + user's custom categories)
    /// </summary>
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public int? UserId { get; set; }
        public bool IsSystemCategory { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public string? CreatedByUserName { get; set; }
    }

    /// <summary>
    /// Request DTO for POST /api/categories/custom (Create custom category)
    /// </summary>
    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Icon cannot exceed 50 characters")]
        public string? Icon { get; set; }

        [StringLength(20, ErrorMessage = "Color code cannot exceed 20 characters")]
        public string? ColorCode { get; set; }
    }

    /// <summary>
    /// Request DTO for PUT /api/categories/custom/{categoryId} (Update custom category)
    /// PUT /api/categories/system/{categoryId} (Update system category - Admin only)
    /// </summary>
    public class UpdateCategoryDTO
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Icon cannot exceed 50 characters")]
        public string? Icon { get; set; }

        [StringLength(20, ErrorMessage = "Color code cannot exceed 20 characters")]
        public string? ColorCode { get; set; }
    }

    /// <summary>
    /// DTO for GET /api/categories/statistics/{userId} (Category-wise spending statistics)
    /// </summary>
    public class CategoryStatisticsDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public bool IsSystemCategory { get; set; }

        // Statistics
        public int ExpenseCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }
}