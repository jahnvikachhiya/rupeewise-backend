using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Category Service Implementation
    /// Handles category management business logic
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// GET /api/categories/my/{userId} - Get system categories + user's custom categories
        /// </summary>
        public async Task<(bool success, string message, List<CategoryDTO>? data)> GetMyCategoriesAsync(int userId)
        {
            try
            {
                var categories = await _categoryRepository.GetCategoriesByUserIdAsync(userId);
                var categoryDtos = categories.Select(MapToCategoryDTO).ToList();
                return (true, "Categories retrieved successfully", categoryDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// POST /api/categories/custom - Create custom category
        /// </summary>
        public async Task<(bool success, string message, CategoryDTO? data)> CreateCustomCategoryAsync(int userId, CreateCategoryDTO request)
        {
            try
            {
                // Check if category name already exists for user
                if (await _categoryRepository.CategoryNameExistsAsync(request.CategoryName, userId))
                {
                    return (false, "Category name already exists", null);
                }

                var category = new Category
                {
                    CategoryName = request.CategoryName,
                    Description = request.Description,
                    Icon = request.Icon,
                    ColorCode = request.ColorCode,
                    UserId = userId,
                    IsSystemCategory = false,
                    CreatedAt = DateTime.UtcNow
                };

                int categoryId = await _categoryRepository.CreateCustomCategoryAsync(category);

                if (categoryId > 0)
                {
                    category.CategoryId = categoryId;
                    var categoryDto = MapToCategoryDTO(category);
                    return (true, "Custom category created successfully", categoryDto);
                }

                return (false, "Failed to create custom category", null);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// POST /api/categories/system - Create system category (Admin only)
        /// </summary>
        public async Task<(bool success, string message, CategoryDTO? data)> CreateSystemCategoryAsync(CreateCategoryDTO request)
        {
            try
            {
                var category = new Category
                {
                    CategoryName = request.CategoryName,
                    Description = request.Description,
                    Icon = request.Icon,
                    ColorCode = request.ColorCode,
                    UserId = null,
                    IsSystemCategory = true,
                    CreatedAt = DateTime.UtcNow
                };

                int categoryId = await _categoryRepository.CreateSystemCategoryAsync(category);

                if (categoryId > 0)
                {
                    category.CategoryId = categoryId;
                    var categoryDto = MapToCategoryDTO(category);
                    return (true, "System category created successfully", categoryDto);
                }

                return (false, "Failed to create system category", null);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// PUT /api/categories/custom/{categoryId} - Update custom category
        /// </summary>
        public async Task<(bool success, string message)> UpdateCustomCategoryAsync(int categoryId, int userId, UpdateCategoryDTO request)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

                if (category == null)
                {
                    return (false, "Category not found");
                }

                // Check if it's a custom category and belongs to the user
                if (category.IsSystemCategory || category.UserId != userId)
                {
                    return (false, "You don't have permission to update this category");
                }

                category.CategoryName = request.CategoryName;
                category.Description = request.Description;
                category.Icon = request.Icon;
                category.ColorCode = request.ColorCode;

                bool updated = await _categoryRepository.UpdateCustomCategoryAsync(category);

                if (updated)
                {
                    return (true, "Custom category updated successfully");
                }

                return (false, "Failed to update custom category");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// PUT /api/categories/system/{categoryId} - Update system category (Admin only)
        /// </summary>
        public async Task<(bool success, string message)> UpdateSystemCategoryAsync(int categoryId, UpdateCategoryDTO request)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

                if (category == null)
                {
                    return (false, "Category not found");
                }

                if (!category.IsSystemCategory)
                {
                    return (false, "This is not a system category");
                }

                category.CategoryName = request.CategoryName;
                category.Description = request.Description;
                category.Icon = request.Icon;
                category.ColorCode = request.ColorCode;

                bool updated = await _categoryRepository.UpdateSystemCategoryAsync(category);

                if (updated)
                {
                    return (true, "System category updated successfully");
                }

                return (false, "Failed to update system category");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE /api/categories/custom/{categoryId} - Delete custom category
        /// </summary>
        public async Task<(bool success, string message)> DeleteCustomCategoryAsync(int categoryId, int userId)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

                if (category == null)
                {
                    return (false, "Category not found");
                }

                // Check if it's a custom category and belongs to the user
                if (category.IsSystemCategory || category.UserId != userId)
                {
                    return (false, "You don't have permission to delete this category");
                }

                // Check if category has expenses
                if (await _categoryRepository.CategoryHasExpensesAsync(categoryId))
                {
                    return (false, "Cannot delete category with existing expenses");
                }

                bool deleted = await _categoryRepository.DeleteCustomCategoryAsync(categoryId);

                if (deleted)
                {
                    return (true, "Custom category deleted successfully");
                }

                return (false, "Failed to delete custom category");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE /api/categories/system/{categoryId} - Delete system category (Admin only)
        /// </summary>
        public async Task<(bool success, string message)> DeleteSystemCategoryAsync(int categoryId)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

                if (category == null)
                {
                    return (false, "Category not found");
                }

                if (!category.IsSystemCategory)
                {
                    return (false, "This is not a system category");
                }

               if (await _categoryRepository.CategoryHasBudgetsAsync(categoryId))
                {
                    return (false, "Cannot delete category because it is used in budgets");
                }

                bool deleted = await _categoryRepository.DeleteSystemCategoryAsync(categoryId);

                if (deleted)
                {
                    return (true, "System category deleted successfully");
                }

                return (false, "Failed to delete system category");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// GET /api/categories/statistics/{userId} - Category-wise spending statistics
        /// </summary>
        public async Task<(bool success, string message, List<CategoryStatisticsDTO>? data)> GetCategoryStatisticsAsync(int userId)
        {
            try
            {
                var statistics = await _categoryRepository.GetCategoryStatisticsAsync(userId);
                return (true, "Category statistics retrieved successfully", statistics);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Helper: Map Category to CategoryDTO
        /// </summary>
        private CategoryDTO MapToCategoryDTO(Category category)
        {
            return new CategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                Icon = category.Icon,
                ColorCode = category.ColorCode,
                UserId = category.UserId,
                IsSystemCategory = category.IsSystemCategory,
                CreatedAt = category.CreatedAt,
                CreatedByUserName = category.CreatedByUserName
            };
        }

            /// <summary>
                /// GET all categories - System + ALL custom categories from all users (Admin only)
                /// </summary>
                public async Task<(bool success, string message, List<CategoryDTO>? data)> GetAllCategoriesAsync()
                {
                    try
                    {
                        var categories = await _categoryRepository.GetAllCategoriesAsync();
                        var categoryDtos = categories.Select(MapToCategoryDTO).ToList();
                        return (true, "All categories retrieved successfully", categoryDtos);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Error: {ex.Message}", null);
                    }
                }
    }
}