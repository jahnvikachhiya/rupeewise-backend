using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Category Service Interface
    /// Defines business logic operations for category management
    /// </summary>
    public interface ICategoryService
    {
        // GET /api/categories/my/{userId}
        Task<(bool success, string message, List<CategoryDTO>? data)> GetMyCategoriesAsync(int userId);

        // POST /api/categories/custom
        Task<(bool success, string message, CategoryDTO? data)> CreateCustomCategoryAsync(int userId, CreateCategoryDTO request);

        // POST /api/categories/system (Admin only)
        Task<(bool success, string message, CategoryDTO? data)> CreateSystemCategoryAsync(CreateCategoryDTO request);

        // PUT /api/categories/custom/{categoryId}
        Task<(bool success, string message)> UpdateCustomCategoryAsync(int categoryId, int userId, UpdateCategoryDTO request);

        // PUT /api/categories/system/{categoryId} (Admin only)
        Task<(bool success, string message)> UpdateSystemCategoryAsync(int categoryId, UpdateCategoryDTO request);

        // DELETE /api/categories/custom/{categoryId}
        Task<(bool success, string message)> DeleteCustomCategoryAsync(int categoryId, int userId);

        // DELETE /api/categories/system/{categoryId} (Admin only)
        Task<(bool success, string message)> DeleteSystemCategoryAsync(int categoryId);

        // GET /api/categories/statistics/{userId}
        Task<(bool success, string message, List<CategoryStatisticsDTO>? data)> GetCategoryStatisticsAsync(int userId);

        // GET all categories (Admin only) - ONLY ONCE!
        Task<(bool success, string message, List<CategoryDTO>? data)> GetAllCategoriesAsync();
    }
}