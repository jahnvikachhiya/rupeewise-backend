using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// Category Repository Interface
    /// Defines all category-related database operations
    /// </summary>
    public interface ICategoryRepository
    {
        // GET /api/categories/my/{userId}
        Task<List<Category>> GetCategoriesByUserIdAsync(int userId);

        // POST /api/categories/custom
        Task<int> CreateCustomCategoryAsync(Category category);

        // POST /api/categories/system (Admin only)
        Task<int> CreateSystemCategoryAsync(Category category);

        // GET category by ID
        Task<Category?> GetCategoryByIdAsync(int categoryId);

        // PUT /api/categories/custom/{categoryId}
        Task<bool> UpdateCustomCategoryAsync(Category category);

        // PUT /api/categories/system/{categoryId} (Admin only)
        Task<bool> UpdateSystemCategoryAsync(Category category);

        // DELETE /api/categories/custom/{categoryId}
        Task<bool> DeleteCustomCategoryAsync(int categoryId);

        // DELETE /api/categories/system/{categoryId} (Admin only)
        Task<bool> DeleteSystemCategoryAsync(int categoryId);

        // GET /api/categories/statistics/{userId}
        Task<List<CategoryStatisticsDTO>> GetCategoryStatisticsAsync(int userId);

        // Helper methods
        Task<bool> CategoryNameExistsAsync(string categoryName, int userId);
        Task<bool> CategoryHasExpensesAsync(int categoryId);
        Task<List<Category>> GetSystemCategoriesAsync();

         Task<List<Category>> GetAllCategoriesAsync();
    }
}