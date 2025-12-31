namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Category entity - Maps to Categories table in database
    /// Table Columns: CategoryId, CategoryName, Description, Icon, ColorCode, UserId, IsSystemCategory, CreatedAt
    /// Foreign Key: UserId → Users.UserId (NULL for system categories)
    /// Supports: System categories (visible to all) and User custom categories (visible to owner only)
    /// </summary>
    public class Category
    {
        /// <summary>
        /// CategoryId (INT, Primary Key, Identity)
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// CategoryName (NVARCHAR(100), NOT NULL)
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Description (NVARCHAR(500))
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Icon (NVARCHAR(50))
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// ColorCode (NVARCHAR(20))
        /// </summary>
        public string? ColorCode { get; set; }

        /// <summary>
        /// UserId (INT, NULL, Foreign Key → Users.UserId)
        /// NULL = System category, UserID = Custom category
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// IsSystemCategory (BIT, DEFAULT 0)
        /// 1 = System (visible to all), 0 = Custom (visible to owner only)
        /// </summary>
        public bool IsSystemCategory { get; set; } = false;

        /// <summary>
        /// CreatedAt (DATETIME, DEFAULT GETDATE())
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property (loaded from JOIN queries)
        public string? CreatedByUserName { get; set; }
    }

    /// <summary>
    /// Default System Categories as per your requirement:
    /// Food & Dining, Transportation, Shopping, Entertainment, Healthcare, Bills & Utilities, Education, Others
    /// </summary>
    public static class DefaultCategories
    {
        public static readonly List<Category> SystemCategories = new()
        {
            new Category 
            { 
                CategoryName = "Food & Dining", 
                Description = "Restaurants, groceries, food delivery", 
                Icon = "🍔", 
                ColorCode = "#FF6B6B", 
                IsSystemCategory = true,
                UserId = null
            },
            new Category 
            { 
                CategoryName = "Transportation", 
                Description = "Fuel, public transport, cab/taxi", 
                Icon = "🚗", 
                ColorCode = "#4ECDC4", 
                IsSystemCategory = true,
                UserId = null
            },
            new Category 
            { 
                CategoryName = "Shopping", 
                Description = "Clothing, electronics, personal items", 
                Icon = "🛍️", 
                ColorCode = "#95E1D3", 
                IsSystemCategory = true,
                UserId = null
            },
            new Category 
            { 
                CategoryName = "Entertainment", 
                Description = "Movies, games, subscriptions", 
                Icon = "🎮", 
                ColorCode = "#F38181", 
                IsSystemCategory = true,
                UserId = null
            },
            new Category 
            { 
                CategoryName = "Healthcare", 
                Description = "Medical expenses, pharmacy, insurance", 
                Icon = "🏥", 
                ColorCode = "#AA96DA", 
                IsSystemCategory = true,
                UserId = null
            },
            new Category 
            { 
                CategoryName = "Bills & Utilities", 
                Description = "Electricity, water, internet, phone", 
                Icon = "💡", 
                ColorCode = "#FCBAD3", 
                IsSystemCategory = true,
                UserId = null
            },
            new Category 
            { 
                CategoryName = "Education", 
                Description = "Courses, books, tuition fees", 
                Icon = "📚", 
                ColorCode = "#FFFFD2", 
                IsSystemCategory = true,
                UserId = null
            },
            new Category 
            { 
                CategoryName = "Others", 
                Description = "Miscellaneous expenses", 
                Icon = "📦", 
                ColorCode = "#A8D8EA", 
                IsSystemCategory = true,
                UserId = null
            }
        };
    }
}