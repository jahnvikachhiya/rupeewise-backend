using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// Category Repository Implementation
    /// Handles all category-related database operations using ADO.NET
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public CategoryRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// GET /api/categories/my/{userId} - Get system + user's custom categories
        /// </summary>
        public async Task<List<Category>> GetCategoriesByUserIdAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() =>
            {
                var categories = new List<Category>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetCategoriesByUserId, parameters);
                while (reader.Read())
                {
                    categories.Add(MapCategoryFromReader(reader));
                }
                return categories;
            });
        }

        /// <summary>
        /// POST /api/categories/custom - Create custom category
        /// </summary>
        public async Task<int> CreateCustomCategoryAsync(Category category)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryName", category.CategoryName },
                { "@Description", category.Description ?? (object)DBNull.Value },
                { "@Icon", category.Icon ?? (object)DBNull.Value },
                { "@ColorCode", category.ColorCode ?? (object)DBNull.Value },
                { "@UserId", category.UserId! }
            };

            return await Task.Run(() => _dbHelper.ExecuteInsertWithId(SqlQueryHelper.InsertCustomCategory, parameters));
        }

        /// <summary>
        /// POST /api/categories/system - Create system category (Admin only)
        /// </summary>
        public async Task<int> CreateSystemCategoryAsync(Category category)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryName", category.CategoryName },
                { "@Description", category.Description ?? (object)DBNull.Value },
                { "@Icon", category.Icon ?? (object)DBNull.Value },
                { "@ColorCode", category.ColorCode ?? (object)DBNull.Value }
            };

            return await Task.Run(() => _dbHelper.ExecuteInsertWithId(SqlQueryHelper.InsertSystemCategory, parameters));
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryId", categoryId }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetCategoryById, parameters);
                if (reader.Read())
                {
                    return MapCategoryFromReader(reader);
                }
                return null;
            });
        }

        /// <summary>
        /// PUT /api/categories/custom/{categoryId} - Update custom category
        /// </summary>
        public async Task<bool> UpdateCustomCategoryAsync(Category category)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryId", category.CategoryId },
                { "@CategoryName", category.CategoryName },
                { "@Description", category.Description ?? (object)DBNull.Value },
                { "@Icon", category.Icon ?? (object)DBNull.Value },
                { "@ColorCode", category.ColorCode ?? (object)DBNull.Value }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.UpdateCustomCategory, parameters) > 0);
        }

        /// <summary>
        /// PUT /api/categories/system/{categoryId} - Update system category (Admin only)
        /// </summary>
        public async Task<bool> UpdateSystemCategoryAsync(Category category)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryId", category.CategoryId },
                { "@CategoryName", category.CategoryName },
                { "@Description", category.Description ?? (object)DBNull.Value },
                { "@Icon", category.Icon ?? (object)DBNull.Value },
                { "@ColorCode", category.ColorCode ?? (object)DBNull.Value }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.UpdateSystemCategory, parameters) > 0);
        }

        /// <summary>
        /// DELETE /api/categories/custom/{categoryId} - Delete custom category
        /// </summary>
        public async Task<bool> DeleteCustomCategoryAsync(int categoryId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryId", categoryId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.DeleteCustomCategory, parameters) > 0);
        }

        /// <summary>
        /// DELETE /api/categories/system/{categoryId} - Delete system category (Admin only)
        /// </summary>
        public async Task<bool> DeleteSystemCategoryAsync(int categoryId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryId", categoryId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.DeleteSystemCategory, parameters) > 0);
        }

        /// <summary>
        /// GET /api/categories/statistics/{userId} - Category-wise spending statistics
        /// </summary>
        public async Task<List<CategoryStatisticsDTO>> GetCategoryStatisticsAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() =>
            {
                var statistics = new List<CategoryStatisticsDTO>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetCategoryStatistics, parameters);
                while (reader.Read())
                {
                    statistics.Add(new CategoryStatisticsDTO
                    {
                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString(reader.GetOrdinal("Icon")),
                        ColorCode = reader.IsDBNull(reader.GetOrdinal("ColorCode")) ? null : reader.GetString(reader.GetOrdinal("ColorCode")),
                        IsSystemCategory = reader.GetBoolean(reader.GetOrdinal("IsSystemCategory")),
                        ExpenseCount = reader.GetInt32(reader.GetOrdinal("ExpenseCount")),
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                        PercentageOfTotal = reader.GetDecimal(reader.GetOrdinal("PercentageOfTotal"))
                    });
                }
                return statistics;
            });
        }

        /// <summary>
        /// Check if category name exists
        /// </summary>
        public async Task<bool> CategoryNameExistsAsync(string categoryName, int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryName", categoryName },
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.RecordExists(SqlQueryHelper.CheckCategoryNameExists, parameters));
        }

        /// <summary>
        /// Check if category has expenses
        /// </summary>
        public async Task<bool> CategoryHasExpensesAsync(int categoryId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@CategoryId", categoryId }
            };

            return await Task.Run(() => _dbHelper.RecordExists(SqlQueryHelper.CheckCategoryHasExpenses, parameters));
        }

        /// <summary>
        /// Get all system categories
        /// </summary>
        public async Task<List<Category>> GetSystemCategoriesAsync()
        {
            return await Task.Run(() =>
            {
                var categories = new List<Category>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetSystemCategories);
                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString(reader.GetOrdinal("Icon")),
                        ColorCode = reader.IsDBNull(reader.GetOrdinal("ColorCode")) ? null : reader.GetString(reader.GetOrdinal("ColorCode")),
                        IsSystemCategory = reader.GetBoolean(reader.GetOrdinal("IsSystemCategory")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
                return categories;
            });
        }

        /// <summary>
        /// Helper method to map SqlDataReader to Category object
        /// </summary>
        private Category MapCategoryFromReader(Microsoft.Data.SqlClient.SqlDataReader reader)
        {
            return new Category
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString(reader.GetOrdinal("Icon")),
                ColorCode = reader.IsDBNull(reader.GetOrdinal("ColorCode")) ? null : reader.GetString(reader.GetOrdinal("ColorCode")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetInt32(reader.GetOrdinal("UserId")),
                IsSystemCategory = reader.GetBoolean(reader.GetOrdinal("IsSystemCategory")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CreatedByUserName = reader.IsDBNull(reader.GetOrdinal("CreatedByUserName")) ? null : reader.GetString(reader.GetOrdinal("CreatedByUserName"))
            };
        }

                /// <summary>
                /// GET all categories - System + ALL custom categories from all users (Admin only)
                /// </summary>
                public async Task<List<Category>> GetAllCategoriesAsync()
                {
                    return await Task.Run(() =>
                    {
                        var categories = new List<Category>();
                        using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetAllCategories);
                        while (reader.Read())
                        {
                            categories.Add(MapCategoryFromReader(reader));
                        }
                        return categories;
                    });
                }

                /// <summary>
/// Check if category is used in budgets
/// </summary>
public async Task<bool> CategoryHasBudgetsAsync(int categoryId)
{
    var parameters = new Dictionary<string, object>
    {
        { "@CategoryId", categoryId }
    };

    return await Task.Run(() =>
        _dbHelper.RecordExists(SqlQueryHelper.CheckCategoryHasBudgets, parameters)
    );
}

    }
}