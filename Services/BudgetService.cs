using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// Budget Service Implementation
    /// Handles budget management business logic and budget alert triggers (80%, 90%, 100%)
    /// </summary>
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly INotificationService _notificationService;
        private readonly ICategoryRepository _categoryRepository;

        public BudgetService(IBudgetRepository budgetRepository, INotificationService notificationService, ICategoryRepository categoryRepository)
        {
            _budgetRepository = budgetRepository;
            _notificationService = notificationService;
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// POST /api/budgets - Create/Set budget
        /// </summary>
        public async Task<(bool success, string message, BudgetDTO? data)> CreateBudgetAsync(int userId, CreateBudgetDTO request)
        {
            try
            {
                // Check if budget already exists
                if (await _budgetRepository.BudgetExistsAsync(userId, request.CategoryId, request.MonthYear))
                {
                    return (false, "Budget already exists for this category and month. Use update instead.", null);
                }

                var budget = new Budget
                {
                    UserId = userId,
                    CategoryId = request.CategoryId,
                    BudgetAmount = request.BudgetAmount,
                    MonthYear = request.MonthYear,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                int budgetId = await _budgetRepository.CreateBudgetAsync(budget);

                if (budgetId > 0)
                {
                    var createdBudget = await _budgetRepository.GetBudgetByIdAsync(budgetId);
                    if (createdBudget != null)
                    {
                        var budgetDto = await MapToBudgetDTOAsync(createdBudget);
                        return (true, "Budget created successfully", budgetDto);
                    }

                    return (true, "Budget created successfully", null);
                }

                return (false, "Failed to create budget", null);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/budgets/my/{userId}?monthYear={monthYear}
        /// </summary>
        public async Task<(bool success, string message, List<BudgetDTO>? data)> GetMyBudgetsAsync(int userId, string monthYear)
        {
            try
            {
                var budgets = await _budgetRepository.GetBudgetsByUserAndMonthAsync(userId, monthYear);
                var budgetDtos = new List<BudgetDTO>();

                foreach (var budget in budgets)
                {
                    budgetDtos.Add(await MapToBudgetDTOAsync(budget));
                }

                return (true, "Budgets retrieved successfully", budgetDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/budgets/{budgetId}
        /// Access: Owner OR Admin
        /// </summary>
        public async Task<(bool success, string message, BudgetDTO? data)> GetBudgetByIdAsync(int budgetId, int requestingUserId, string userRole)
        {
            try
            {
                var budget = await _budgetRepository.GetBudgetByIdAsync(budgetId);

                if (budget == null)
                {
                    return (false, "Budget not found", null);
                }

                // Check access: Owner or Admin
                if (budget.UserId != requestingUserId && userRole != "Admin")
                {
                    return (false, "You don't have permission to view this budget", null);
                }

                var budgetDto = await MapToBudgetDTOAsync(budget);
                return (true, "Budget retrieved successfully", budgetDto);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// PUT /api/budgets/{budgetId} - Update budget
        /// Access: Owner OR Admin
        /// </summary>
        public async Task<(bool success, string message)> UpdateBudgetAsync(int budgetId, int requestingUserId, string userRole, UpdateBudgetDTO request)
        {
            try
            {
                var budget = await _budgetRepository.GetBudgetByIdAsync(budgetId);

                if (budget == null)
                {
                    return (false, "Budget not found");
                }

                // Check access: Owner or Admin
                if (budget.UserId != requestingUserId && userRole != "Admin")
                {
                    return (false, "You don't have permission to update this budget");
                }

                bool updated = await _budgetRepository.UpdateBudgetAsync(budgetId, request.BudgetAmount);

                if (updated)
                {
                    // Re-check budget after update
                    await CheckBudgetAndSendAlertsAsync(budget.UserId, budget.CategoryId, budget.MonthYear);
                    return (true, "Budget updated successfully");
                }

                return (false, "Failed to update budget");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE /api/budgets/{budgetId}
        /// Access: Owner OR Admin
        /// </summary>
        public async Task<(bool success, string message)> DeleteBudgetAsync(int budgetId, int requestingUserId, string userRole)
        {
            try
            {
                var budget = await _budgetRepository.GetBudgetByIdAsync(budgetId);

                if (budget == null)
                {
                    return (false, "Budget not found");
                }

                // Check access: Owner or Admin
                if (budget.UserId != requestingUserId && userRole != "Admin")
                {
                    return (false, "You don't have permission to delete this budget");
                }

                bool deleted = await _budgetRepository.DeleteBudgetAsync(budgetId);

                if (deleted)
                {
                    return (true, "Budget deleted successfully");
                }

                return (false, "Failed to delete budget");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// GET /api/budgets/vs-actual/{userId} - Budget vs Actual spending
        /// </summary>
        public async Task<(bool success, string message, BudgetVsActualDTO? data)> GetBudgetVsActualAsync(int userId, int? categoryId, string monthYear)
        {
            try
            {
                var budgetVsActual = await _budgetRepository.GetBudgetVsActualAsync(userId, categoryId, monthYear);

                if (budgetVsActual == null)
                {
                    return (false, "Budget not found for specified category and month", null);
                }

                return (true, "Budget vs actual retrieved successfully", budgetVsActual);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/budgets/status/{userId} - Check budget status and percentage used
        /// </summary>
        public async Task<(bool success, string message, BudgetStatusDTO? data)> GetBudgetStatusAsync(int userId, int? categoryId, string monthYear)
        {
            try
            {
                var budgetStatus = await _budgetRepository.GetBudgetStatusAsync(userId, categoryId, monthYear);

                if (budgetStatus == null)
                {
                    return (false, "Budget not found for specified category and month", null);
                }

                return (true, "Budget status retrieved successfully", budgetStatus);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Check budget and send alerts if thresholds reached (80%, 90%, 100%)
        /// Auto-triggered by: Expense creation, Expense update, Budget update
        /// Manual trigger: POST /api/notifications/send-budget-alert
        /// </summary>
        public async Task CheckBudgetAndSendAlertsAsync(int userId, int? categoryId, string monthYear)
        {
            try
            {
                var budgetStatus = await _budgetRepository.GetBudgetStatusAsync(userId, categoryId, monthYear);

                if (budgetStatus == null)
                    return;

                decimal percentageUsed = budgetStatus.PercentageUsed;
                string categoryName = budgetStatus.CategoryName;

                // Send alert based on percentage thresholds
                if (percentageUsed >= 100)
                {
                    // 🔴 Alert: Budget Exceeded (100%)
                    await _notificationService.SendBudgetAlertAsync(userId, categoryName, percentageUsed, 
                        budgetStatus.BudgetAmount, budgetStatus.CurrentSpending, "Alert");
                }
                else if (percentageUsed >= 90)
                {
                    // 🟡 Warning: 90% threshold
                    await _notificationService.SendBudgetAlertAsync(userId, categoryName, percentageUsed, 
                        budgetStatus.BudgetAmount, budgetStatus.CurrentSpending, "Warning");
                }
                else if (percentageUsed >= 80)
                {
                    // 🔵 Info: 80% threshold
                    await _notificationService.SendBudgetAlertAsync(userId, categoryName, percentageUsed, 
                        budgetStatus.BudgetAmount, budgetStatus.CurrentSpending, "Info");
                }
            }
            catch
            {
                // Silently fail - don't block expense creation if notification fails
            }
        }

        /// <summary>
        /// Helper: Map Budget to BudgetDTO with current spending
        /// </summary>
        private async Task<BudgetDTO> MapToBudgetDTOAsync(Budget budget)
        {
            decimal currentSpending = await _budgetRepository.GetCurrentSpendingAsync(budget.UserId, budget.CategoryId, budget.MonthYear);
            decimal remaining = budget.BudgetAmount - currentSpending;
            decimal percentageUsed = budget.BudgetAmount > 0 ? (currentSpending / budget.BudgetAmount) * 100 : 0;

            string status = "On Track";
            if (currentSpending > budget.BudgetAmount)
                status = "Exceeded";
            else if (percentageUsed >= 90)
                status = "Critical";
            else if (percentageUsed >= 80)
                status = "Warning";
            string? categoryIcon = null;

if (budget.CategoryId.HasValue)
{
    var category = await _categoryRepository.GetCategoryByIdAsync(budget.CategoryId.Value);
    categoryIcon = category?.Icon;
}
            return new BudgetDTO
            {
                BudgetId = budget.BudgetId,
                UserId = budget.UserId,
                CategoryId = budget.CategoryId,
            
                BudgetAmount = budget.BudgetAmount,
                MonthYear = budget.MonthYear,
                CreatedAt = budget.CreatedAt,
                UpdatedAt = budget.UpdatedAt,
                CategoryName = budget.CategoryName,
                CategoryIcon = categoryIcon, // ✅ FIX
                UserFullName = budget.UserFullName ?? string.Empty,
                CurrentSpending = currentSpending,
                RemainingBudget = remaining,
                PercentageUsed = percentageUsed,
                BudgetStatus = status
            };
        }
    }
}