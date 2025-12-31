namespace ExpenseManagementAPI.Data
{
    /// <summary>
    /// SQL Query Helper - Contains ALL SQL queries as constants
    /// Uses parameterized queries to prevent SQL injection
    /// Part 1: User Management & Authentication Queries
    /// </summary>
    public static partial class SqlQueryHelper
    {
        // ========================================
        // USER MANAGEMENT QUERIES
        // ========================================

        /// <summary>
        /// POST /api/users/register - Insert new user
        /// Parameters: @Username, @Email, @PasswordHash, @FullName, @PhoneNumber, @Role
        /// </summary>
        public const string InsertUser = @"
            INSERT INTO Users (Username, Email, PasswordHash, FullName, PhoneNumber, Role, CreatedAt, IsActive)
            VALUES (@Username, @Email, @PasswordHash, @FullName, @PhoneNumber, @Role, GETDATE(), 1)";

        /// <summary>
        /// Check if username already exists
        /// Parameters: @Username
        /// </summary>
        public const string CheckUsernameExists = @"
            SELECT COUNT(*) FROM Users WHERE Username = @Username";

        /// <summary>
        /// Check if email already exists
        /// Parameters: @Email
        /// </summary>
        public const string CheckEmailExists = @"
            SELECT COUNT(*) FROM Users WHERE Email = @Email";

        /// <summary>
        /// POST /api/users/login - Get user by username or email for login
        /// Parameters: @UsernameOrEmail
        /// </summary>
        public const string GetUserByUsernameOrEmail = @"
            SELECT UserId, Username, Email, PasswordHash, FullName, PhoneNumber, Role, CreatedAt, IsActive
            FROM Users
            WHERE (Username = @UsernameOrEmail OR Email = @UsernameOrEmail) AND IsActive = 1";

        /// <summary>
        /// GET /api/users/profile/{userId} - Get user by ID
        /// Parameters: @UserId
        /// </summary>
        public const string GetUserById = @"
            SELECT UserId, Username, Email, PasswordHash, FullName, PhoneNumber, Role, CreatedAt, IsActive
            FROM Users
            WHERE UserId = @UserId";

        /// <summary>
        /// GET /api/users - Get all users (Admin only)
        /// No parameters
        /// </summary>
        public const string GetAllUsers = @"
            SELECT UserId, Username, Email, FullName, PhoneNumber, Role, CreatedAt, IsActive
            FROM Users
            ORDER BY CreatedAt DESC";

        /// <summary>
        /// PUT /api/users/profile/{userId} - Update user profile
        /// Parameters: @UserId, @FullName, @Email, @PhoneNumber
        /// </summary>
        public const string UpdateUserProfile = @"
            UPDATE Users
            SET FullName = @FullName, Email = @Email, PhoneNumber = @PhoneNumber
            WHERE UserId = @UserId";

        /// <summary>
        /// PUT /api/users/change-password/{userId} - Change user password
        /// Parameters: @UserId, @PasswordHash
        /// </summary>
        public const string ChangePassword = @"
            UPDATE Users
            SET PasswordHash = @PasswordHash
            WHERE UserId = @UserId";

        /// <summary>
        /// PUT /api/users/deactivate/{userId} - Deactivate user (Admin only)
        /// Parameters: @UserId
        /// </summary>
        public const string DeactivateUser = @"
            UPDATE Users
            SET IsActive = 0
            WHERE UserId = @UserId";

        /// <summary>
        /// Activate user (Admin only)
        /// Parameters: @UserId
        /// </summary>
        public const string ActivateUser = @"
            UPDATE Users
            SET IsActive = 1
            WHERE UserId = @UserId";

        /// <summary>
        /// Check if email exists for other user (for update validation)
        /// Parameters: @Email, @UserId
        /// </summary>
        public const string CheckEmailExistsForOtherUser = @"
            SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserId != @UserId";
        
    }
}
namespace ExpenseManagementAPI.Data
{
    /// <summary>
    /// Part 2: Expense Management Queries
    /// </summary>
    public static partial class SqlQueryHelper
    {
        // ========================================
        // EXPENSE MANAGEMENT QUERIES
        // ========================================

        /// <summary>
        /// POST /api/expenses - Insert new expense
        /// Parameters: @UserId, @CategoryId, @Amount, @ExpenseDate, @Description, @PaymentMethod, @Status
        /// </summary>
        public const string InsertExpense = @"
            INSERT INTO Expenses (UserId, CategoryId, Amount, ExpenseDate, Description, PaymentMethod, Status, CreatedAt, UpdatedAt)
            VALUES (@UserId, @CategoryId, @Amount, @ExpenseDate, @Description, @PaymentMethod, @Status, GETDATE(), GETDATE())";

        /// <summary>
        /// GET /api/expenses/my/{userId} - Get all expenses for a user
        /// Parameters: @UserId
        /// </summary>
        public const string GetExpensesByUserId = @"
            SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description, 
                   e.PaymentMethod, e.Status, e.CreatedAt, e.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName , u.Username 
            FROM Expenses e
            INNER JOIN Categories c ON e.CategoryId = c.CategoryId
            INNER JOIN Users u ON e.UserId = u.UserId
            WHERE e.UserId = @UserId
            ORDER BY e.ExpenseDate DESC, e.CreatedAt DESC";

        /// <summary>
        /// GET /api/expenses/{expenseId} - Get expense by ID
        /// Parameters: @ExpenseId
        /// </summary>
        public const string GetExpenseById = @"
            SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description, 
                   e.PaymentMethod, e.Status, e.CreatedAt, e.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName ,u.Username 
            FROM Expenses e
            INNER JOIN Categories c ON e.CategoryId = c.CategoryId
            INNER JOIN Users u ON e.UserId = u.UserId
            WHERE e.ExpenseId = @ExpenseId";

        /// <summary>
        /// PUT /api/expenses/{expenseId} - Update expense
        /// Parameters: @ExpenseId, @CategoryId, @Amount, @ExpenseDate, @Description, @PaymentMethod
        /// </summary>
        public const string UpdateExpense = @"
            UPDATE Expenses
            SET CategoryId = @CategoryId, Amount = @Amount, ExpenseDate = @ExpenseDate, 
                Description = @Description, PaymentMethod = @PaymentMethod, UpdatedAt = GETDATE()
            WHERE ExpenseId = @ExpenseId";

        /// <summary>
        /// DELETE /api/expenses/{expenseId} - Delete expense
        /// Parameters: @ExpenseId
        /// </summary>
        public const string DeleteExpense = @"
            DELETE FROM Expenses WHERE ExpenseId = @ExpenseId";

        /// <summary>
        /// GET /api/expenses/all - Get all expenses (Admin only)
        /// No parameters
        /// </summary>
        public const string GetAllExpenses = @"
            SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description, 
                   e.PaymentMethod, e.Status, e.CreatedAt, e.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName ,u.Username 
            FROM Expenses e
            INNER JOIN Categories c ON e.CategoryId = c.CategoryId
            INNER JOIN Users u ON e.UserId = u.UserId
            ORDER BY e.ExpenseDate DESC, e.CreatedAt DESC";

        /// <summary>
        /// GET /api/expenses/filter - Base query for expense filtering
        /// Dynamic WHERE clause will be built in repository based on filters
        /// </summary>
        public const string FilterExpensesBase = @"
            SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description, 
                   e.PaymentMethod, e.Status, e.CreatedAt, e.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName ,u.Username 
            FROM Expenses e
            INNER JOIN Categories c ON e.CategoryId = c.CategoryId
            INNER JOIN Users u ON e.UserId = u.UserId
            WHERE e.UserId = @UserId";

        /// <summary>
        /// GET /api/expenses/search - Search expenses by description or amount
        /// Parameters: @UserId, @SearchTerm
        /// </summary>
        public const string SearchExpenses = @"
            SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description, 
                   e.PaymentMethod, e.Status, e.CreatedAt, e.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName ,u.Username 
            FROM Expenses e
            INNER JOIN Categories c ON e.CategoryId = c.CategoryId
            INNER JOIN Users u ON e.UserId = u.UserId
            WHERE e.UserId = @UserId 
            AND (e.Description LIKE '%' + @SearchTerm + '%' OR CAST(e.Amount AS NVARCHAR) LIKE '%' + @SearchTerm + '%')
            ORDER BY e.ExpenseDate DESC";

        /// <summary>
        /// Get total expense count for a user
        /// Parameters: @UserId
        /// </summary>
        public const string GetExpenseCountByUser = @"
            SELECT COUNT(*) FROM Expenses WHERE UserId = @UserId";

        /// <summary>
        /// Get total spending for a user
        /// Parameters: @UserId
        /// </summary>
        public const string GetTotalSpendingByUser = @"
            SELECT ISNULL(SUM(Amount), 0) FROM Expenses WHERE UserId = @UserId";

        public static readonly string FilterExpenses = @"
        SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description,
               e.PaymentMethod, e.Status, e.CreatedAt, e.UpdatedAt,
               c.CategoryName, u.FullName AS UserFullName
        FROM Expenses e
        INNER JOIN Categories c ON e.CategoryId = c.CategoryId
        INNER JOIN Users u ON e.UserId = u.UserId
        WHERE (@UserId IS NULL OR e.UserId = @UserId)
          AND (@CategoryId IS NULL OR e.CategoryId = @CategoryId)
          AND (@StartDate IS NULL OR e.ExpenseDate >= @StartDate)
          AND (@EndDate IS NULL OR e.ExpenseDate <= @EndDate)
          AND (@PaymentMethod IS NULL OR e.PaymentMethod = @PaymentMethod)
          AND (@Status IS NULL OR e.Status = @Status)
          AND (@SearchText IS NULL OR e.Description LIKE '%' + @SearchText + '%')
        ORDER BY e.ExpenseDate DESC";
    }
}

namespace ExpenseManagementAPI.Data
{
    /// <summary>
    /// Part 3: Category Management Queries
    /// </summary>
    public static partial class SqlQueryHelper
    {
        // ========================================
        // CATEGORY MANAGEMENT QUERIES
        // ========================================

        /// <summary>
        /// GET /api/categories/my/{userId} - Get system categories + user's custom categories
        /// Parameters: @UserId
        /// </summary>
        public const string GetCategoriesByUserId = @"
            SELECT c.CategoryId, c.CategoryName, c.Description, c.Icon, c.ColorCode, 
                   c.UserId, c.IsSystemCategory, c.CreatedAt,
                   u.FullName AS CreatedByUserName
            FROM Categories c
            LEFT JOIN Users u ON c.UserId = u.UserId
            WHERE c.IsSystemCategory = 1 OR c.UserId = @UserId
            ORDER BY c.IsSystemCategory DESC, c.CategoryName ASC";

        /// <summary>
        /// POST /api/categories/custom - Insert custom category
        /// Parameters: @CategoryName, @Description, @Icon, @ColorCode, @UserId
        /// </summary>
        public const string InsertCustomCategory = @"
            INSERT INTO Categories (CategoryName, Description, Icon, ColorCode, UserId, IsSystemCategory, CreatedAt)
            VALUES (@CategoryName, @Description, @Icon, @ColorCode, @UserId, 0, GETDATE())";

        /// <summary>
        /// POST /api/categories/system - Insert system category (Admin only)
        /// Parameters: @CategoryName, @Description, @Icon, @ColorCode
        /// </summary>
        public const string InsertSystemCategory = @"
            INSERT INTO Categories (CategoryName, Description, Icon, ColorCode, UserId, IsSystemCategory, CreatedAt)
            VALUES (@CategoryName, @Description, @Icon, @ColorCode, NULL, 1, GETDATE())";

        /// <summary>
        /// GET category by ID
        /// Parameters: @CategoryId
        /// </summary>
        public const string GetCategoryById = @"
            SELECT c.CategoryId, c.CategoryName, c.Description, c.Icon, c.ColorCode, 
                   c.UserId, c.IsSystemCategory, c.CreatedAt,
                   u.FullName AS CreatedByUserName
            FROM Categories c
            LEFT JOIN Users u ON c.UserId = u.UserId
            WHERE c.CategoryId = @CategoryId";

        /// <summary>
        /// PUT /api/categories/custom/{categoryId} - Update custom category
        /// Parameters: @CategoryId, @CategoryName, @Description, @Icon, @ColorCode
        /// </summary>
        public const string UpdateCustomCategory = @"
            UPDATE Categories
            SET CategoryName = @CategoryName, Description = @Description, 
                Icon = @Icon, ColorCode = @ColorCode
            WHERE CategoryId = @CategoryId AND IsSystemCategory = 0";

        /// <summary>
        /// PUT /api/categories/system/{categoryId} - Update system category (Admin only)
        /// Parameters: @CategoryId, @CategoryName, @Description, @Icon, @ColorCode
        /// </summary>
        public const string UpdateSystemCategory = @"
            UPDATE Categories
            SET CategoryName = @CategoryName, Description = @Description, 
                Icon = @Icon, ColorCode = @ColorCode
            WHERE CategoryId = @CategoryId AND IsSystemCategory = 1";

        /// <summary>
        /// DELETE /api/categories/custom/{categoryId} - Delete custom category
        /// Parameters: @CategoryId
        /// </summary>
        public const string DeleteCustomCategory = @"
            DELETE FROM Categories WHERE CategoryId = @CategoryId AND IsSystemCategory = 0";

        /// <summary>
        /// DELETE /api/categories/system/{categoryId} - Delete system category (Admin only)
        /// Parameters: @CategoryId
        /// </summary>
        public const string DeleteSystemCategory = @"
            DELETE FROM Categories WHERE CategoryId = @CategoryId AND IsSystemCategory = 1";

        /// <summary>
        /// GET /api/categories/statistics/{userId} - Category-wise spending statistics
        /// Parameters: @UserId
        /// </summary>
        public const string GetCategoryStatistics = @"
            SELECT c.CategoryId, c.CategoryName, c.Icon, c.ColorCode, c.IsSystemCategory,
                   COUNT(e.ExpenseId) AS ExpenseCount,
                   ISNULL(SUM(e.Amount), 0) AS TotalAmount,
                   CASE 
                       WHEN (SELECT SUM(Amount) FROM Expenses WHERE UserId = @UserId) > 0 
                       THEN (ISNULL(SUM(e.Amount), 0) * 100.0 / (SELECT SUM(Amount) FROM Expenses WHERE UserId = @UserId))
                       ELSE 0
                   END AS PercentageOfTotal
            FROM Categories c
            LEFT JOIN Expenses e ON c.CategoryId = e.CategoryId AND e.UserId = @UserId
            WHERE c.IsSystemCategory = 1 OR c.UserId = @UserId
            GROUP BY c.CategoryId, c.CategoryName, c.Icon, c.ColorCode, c.IsSystemCategory
            HAVING COUNT(e.ExpenseId) > 0
            ORDER BY TotalAmount DESC";

        /// <summary>
        /// Check if category name already exists for user
        /// Parameters: @CategoryName, @UserId
        /// </summary>
        public const string CheckCategoryNameExists = @"
            SELECT COUNT(*) FROM Categories 
            WHERE CategoryName = @CategoryName AND (IsSystemCategory = 1 OR UserId = @UserId)";

        /// <summary>
        /// Check if category has expenses
        /// Parameters: @CategoryId
        /// </summary>
        public const string CheckCategoryHasExpenses = @"
            SELECT COUNT(*) FROM Expenses WHERE CategoryId = @CategoryId";

        /// <summary>
        /// Get all system categories
        /// No parameters
        /// </summary>
        public const string GetSystemCategories = @"
            SELECT CategoryId, CategoryName, Description, Icon, ColorCode, IsSystemCategory, CreatedAt
            FROM Categories
            WHERE IsSystemCategory = 1
            ORDER BY CategoryName ASC";


          /// <summary>
    /// GET all categories (System + ALL custom from all users) - Admin only
    /// </summary>
    public static readonly string GetAllCategories = @"
        SELECT 
            c.CategoryId,
            c.CategoryName,
            c.Description,
            c.Icon,
            c.ColorCode,
            c.UserId,
            c.IsSystemCategory,
            c.CreatedAt,
            u.Username AS CreatedByUserName
        FROM Categories c
        LEFT JOIN Users u ON c.UserId = u.UserId
        ORDER BY c.IsSystemCategory DESC, c.CategoryName ASC";
    }
}

namespace ExpenseManagementAPI.Data
{
    /// <summary>
    /// Part 4: Budget Management Queries
    /// </summary>
    public static partial class SqlQueryHelper
    {
        // ========================================
        // BUDGET MANAGEMENT QUERIES
        // ========================================

        /// <summary>
        /// POST /api/budgets - Insert/Set budget
        /// Parameters: @UserId, @CategoryId, @BudgetAmount, @MonthYear
        /// </summary>
        public const string InsertBudget = @"
            INSERT INTO Budgets (UserId, CategoryId, BudgetAmount, MonthYear, CreatedAt, UpdatedAt)
            VALUES (@UserId, @CategoryId, @BudgetAmount, @MonthYear, GETDATE(), GETDATE())";

        /// <summary>
        /// GET /api/budgets/my/{userId}?monthYear={monthYear} - Get user's budgets for a month
        /// Parameters: @UserId, @MonthYear
        /// </summary>
        public const string GetBudgetsByUserAndMonth = @"
            SELECT b.BudgetId, b.UserId, b.CategoryId, b.BudgetAmount, b.MonthYear, 
                   b.CreatedAt, b.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName
            FROM Budgets b
            LEFT JOIN Categories c ON b.CategoryId = c.CategoryId
            INNER JOIN Users u ON b.UserId = u.UserId
            WHERE b.UserId = @UserId AND b.MonthYear = @MonthYear
            ORDER BY b.CategoryId ASC";

        /// <summary>
        /// GET /api/budgets/{budgetId} - Get budget by ID
        /// Parameters: @BudgetId
        /// </summary>
        public const string GetBudgetById = @"
            SELECT b.BudgetId, b.UserId, b.CategoryId, b.BudgetAmount, b.MonthYear, 
                   b.CreatedAt, b.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName
            FROM Budgets b
            LEFT JOIN Categories c ON b.CategoryId = c.CategoryId
            INNER JOIN Users u ON b.UserId = u.UserId
            WHERE b.BudgetId = @BudgetId";

        /// <summary>
        /// PUT /api/budgets/{budgetId} - Update budget
        /// Parameters: @BudgetId, @BudgetAmount
        /// </summary>
        public const string UpdateBudget = @"
            UPDATE Budgets
            SET BudgetAmount = @BudgetAmount, UpdatedAt = GETDATE()
            WHERE BudgetId = @BudgetId";

        /// <summary>
        /// DELETE /api/budgets/{budgetId} - Delete budget
        /// Parameters: @BudgetId
        /// </summary>
        public const string DeleteBudget = @"
            DELETE FROM Budgets WHERE BudgetId = @BudgetId";

        /// <summary>
        /// Check if budget already exists for user, category, and month
        /// Parameters: @UserId, @CategoryId, @MonthYear
        /// </summary>
        public const string CheckBudgetExists = @"
            SELECT COUNT(*) FROM Budgets 
            WHERE UserId = @UserId 
            AND (@CategoryId IS NULL AND CategoryId IS NULL OR CategoryId = @CategoryId)
            AND MonthYear = @MonthYear";

        /// <summary>
        /// Get budget ID if exists
        /// Parameters: @UserId, @CategoryId, @MonthYear
        /// </summary>
        public const string GetBudgetIdIfExists = @"
            SELECT BudgetId FROM Budgets 
            WHERE UserId = @UserId 
            AND (@CategoryId IS NULL AND CategoryId IS NULL OR CategoryId = @CategoryId)
            AND MonthYear = @MonthYear";

        /// <summary>
        /// GET /api/budgets/vs-actual/{userId} - Budget vs Actual spending
        /// Parameters: @UserId, @CategoryId, @MonthYear
        /// </summary>
        public const string GetBudgetVsActual = @"
            SELECT 
                b.BudgetId, b.CategoryId, c.CategoryName, b.MonthYear, b.BudgetAmount,
                ISNULL(SUM(e.Amount), 0) AS ActualSpending,
                COUNT(e.ExpenseId) AS ExpenseCount
            FROM Budgets b
            LEFT JOIN Categories c ON b.CategoryId = c.CategoryId
            LEFT JOIN Expenses e ON b.UserId = e.UserId 
                AND (b.CategoryId IS NULL OR b.CategoryId = e.CategoryId)
                AND FORMAT(e.ExpenseDate, 'yyyy-MM') = b.MonthYear
            WHERE b.UserId = @UserId 
            AND (@CategoryId IS NULL AND b.CategoryId IS NULL OR b.CategoryId = @CategoryId)
            AND b.MonthYear = @MonthYear
            GROUP BY b.BudgetId, b.CategoryId, c.CategoryName, b.MonthYear, b.BudgetAmount";

        /// <summary>
        /// GET /api/budgets/status/{userId} - Check budget status
        /// Parameters: @UserId, @CategoryId, @MonthYear
        /// </summary>
        public const string GetBudgetStatus = @"
            SELECT 
                b.BudgetId, b.UserId, b.CategoryId, c.CategoryName, b.MonthYear, b.BudgetAmount,
                ISNULL(SUM(e.Amount), 0) AS CurrentSpending
            FROM Budgets b
            LEFT JOIN Categories c ON b.CategoryId = c.CategoryId
            LEFT JOIN Expenses e ON b.UserId = e.UserId 
                AND (b.CategoryId IS NULL OR b.CategoryId = e.CategoryId)
                AND FORMAT(e.ExpenseDate, 'yyyy-MM') = b.MonthYear
            WHERE b.UserId = @UserId 
            AND (@CategoryId IS NULL AND b.CategoryId IS NULL OR b.CategoryId = @CategoryId)
            AND b.MonthYear = @MonthYear
            GROUP BY b.BudgetId, b.UserId, b.CategoryId, c.CategoryName, b.MonthYear, b.BudgetAmount";

        /// <summary>
        /// Get current spending for a budget
        /// Parameters: @UserId, @CategoryId, @MonthYear
        /// </summary>
        public const string GetCurrentSpending = @"
            SELECT ISNULL(SUM(e.Amount), 0)
            FROM Expenses e
            WHERE e.UserId = @UserId 
            AND (@CategoryId IS NULL OR e.CategoryId = @CategoryId)
            AND FORMAT(e.ExpenseDate, 'yyyy-MM') = @MonthYear";

        /// <summary>
        /// Get all budgets for a user (all months)
        /// Parameters: @UserId
        /// </summary>
        public const string GetAllBudgetsByUser = @"
            SELECT b.BudgetId, b.UserId, b.CategoryId, b.BudgetAmount, b.MonthYear, 
                   b.CreatedAt, b.UpdatedAt,
                   c.CategoryName, u.FullName AS UserFullName
            FROM Budgets b
            LEFT JOIN Categories c ON b.CategoryId = c.CategoryId
            INNER JOIN Users u ON b.UserId = u.UserId
            WHERE b.UserId = @UserId
            ORDER BY b.MonthYear DESC, b.CategoryId ASC";
    }
}

namespace ExpenseManagementAPI.Data
{
    /// <summary>
    /// Part 5: Notification & Dashboard Queries
    /// </summary>
    public static partial class SqlQueryHelper
    {
        // ========================================
        // NOTIFICATION QUERIES
        // ========================================

        /// <summary>
        /// POST notification - Insert notification
        /// Parameters: @UserId, @Title, @Message, @Type
        /// </summary>
        public const string InsertNotification = @"
            INSERT INTO Notifications (UserId, Title, Message, Type, IsRead, CreatedAt)
            VALUES (@UserId, @Title, @Message, @Type, 0, GETDATE())";

        /// <summary>
        /// GET /api/notifications/my/{userId} - Get user's notifications
        /// Parameters: @UserId
        /// </summary>
        public const string GetNotificationsByUserId = @"
            SELECT n.NotificationId, n.UserId, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt, n.ReadAt,
                   u.FullName AS UserFullName
            FROM Notifications n
            INNER JOIN Users u ON n.UserId = u.UserId
            WHERE n.UserId = @UserId
            ORDER BY n.CreatedAt DESC";

        /// <summary>
        /// GET /api/notifications/unread-count/{userId} - Get unread notification count
        /// Parameters: @UserId
        /// </summary>
        public const string GetUnreadNotificationCount = @"
            SELECT COUNT(*) FROM Notifications 
            WHERE UserId = @UserId AND IsRead = 0";

        /// <summary>
        /// PUT /api/notifications/mark-read/{notificationId} - Mark notification as read
        /// Parameters: @NotificationId
        /// </summary>
        public const string MarkNotificationAsRead = @"
            UPDATE Notifications
            SET IsRead = 1, ReadAt = GETDATE()
            WHERE NotificationId = @NotificationId";

        /// <summary>
        /// DELETE /api/notifications/{notificationId} - Delete notification
        /// Parameters: @NotificationId
        /// </summary>
        public const string DeleteNotification = @"
            DELETE FROM Notifications WHERE NotificationId = @NotificationId";

        /// <summary>
        /// Get notification by ID
        /// Parameters: @NotificationId
        /// </summary>
        public const string GetNotificationById = @"
            SELECT n.NotificationId, n.UserId, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt, n.ReadAt,
                   u.FullName AS UserFullName
            FROM Notifications n
            INNER JOIN Users u ON n.UserId = u.UserId
            WHERE n.NotificationId = @NotificationId";

        // ========================================
        // DASHBOARD & REPORTS QUERIES
        // ========================================

        /// <summary>
        /// GET /api/dashboard/summary/{userId} - Dashboard summary
        /// Parameters: @UserId
        /// </summary>
        public const string GetDashboardSummary = @"
            SELECT 
                @UserId AS UserId,
                u.FullName AS UserFullName,
                ISNULL(SUM(e.Amount), 0) AS TotalExpenses,
                COUNT(e.ExpenseId) AS TotalExpenseCount,
                ISNULL(SUM(CASE WHEN FORMAT(e.ExpenseDate, 'yyyy-MM') = FORMAT(GETDATE(), 'yyyy-MM') THEN e.Amount ELSE 0 END), 0) AS MonthlyExpenses,
                SUM(CASE WHEN FORMAT(e.ExpenseDate, 'yyyy-MM') = FORMAT(GETDATE(), 'yyyy-MM') THEN 1 ELSE 0 END) AS MonthlyExpenseCount,
                ISNULL(SUM(CASE WHEN CAST(e.ExpenseDate AS DATE) = CAST(GETDATE() AS DATE) THEN e.Amount ELSE 0 END), 0) AS TodayExpenses,
                SUM(CASE WHEN CAST(e.ExpenseDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodayExpenseCount
            FROM Users u
            LEFT JOIN Expenses e ON u.UserId = e.UserId
            WHERE u.UserId = @UserId
            GROUP BY u.UserId, u.FullName";

        /// <summary>
        /// GET /api/dashboard/category-breakdown/{userId} - Category breakdown
        /// Parameters: @UserId, @StartDate, @EndDate
        /// </summary>
        public const string GetCategoryBreakdown = @"
            SELECT c.CategoryId, c.CategoryName, c.Icon, c.ColorCode,
                   ISNULL(SUM(e.Amount), 0) AS Amount,
                   COUNT(e.ExpenseId) AS ExpenseCount,
                   CASE 
                       WHEN (SELECT SUM(Amount) FROM Expenses WHERE UserId = @UserId 
                             AND ExpenseDate >= @StartDate AND ExpenseDate <= @EndDate) > 0 
                       THEN (ISNULL(SUM(e.Amount), 0) * 100.0 / 
                             (SELECT SUM(Amount) FROM Expenses WHERE UserId = @UserId 
                              AND ExpenseDate >= @StartDate AND ExpenseDate <= @EndDate))
                       ELSE 0
                   END AS PercentageOfTotal
            FROM Categories c
            LEFT JOIN Expenses e ON c.CategoryId = e.CategoryId 
                AND e.UserId = @UserId 
                AND e.ExpenseDate >= @StartDate 
                AND e.ExpenseDate <= @EndDate
            WHERE c.IsSystemCategory = 1 OR c.UserId = @UserId
            GROUP BY c.CategoryId, c.CategoryName, c.Icon, c.ColorCode
            HAVING COUNT(e.ExpenseId) > 0
            ORDER BY Amount DESC";

        /// <summary>
        /// GET /api/dashboard/monthly-trend/{userId} - Monthly trend
        /// Parameters: @UserId, @Year
        /// </summary>
        public const string GetMonthlyTrend = @"
            SELECT 
                FORMAT(e.ExpenseDate, 'yyyy-MM') AS MonthYear,
                SUM(e.Amount) AS Amount,
                COUNT(e.ExpenseId) AS ExpenseCount
            FROM Expenses e
            WHERE e.UserId = @UserId AND YEAR(e.ExpenseDate) = @Year
            GROUP BY FORMAT(e.ExpenseDate, 'yyyy-MM')
            ORDER BY MonthYear ASC";

        /// <summary>
        /// GET /api/dashboard/budget-progress/{userId} - Budget progress
        /// Parameters: @UserId, @MonthYear
        /// </summary>
        public const string GetBudgetProgress = @"
            SELECT 
                b.BudgetId, b.CategoryId, c.CategoryName, b.BudgetAmount,
                ISNULL(SUM(e.Amount), 0) AS CurrentSpending
            FROM Budgets b
            LEFT JOIN Categories c ON b.CategoryId = c.CategoryId
            LEFT JOIN Expenses e ON b.UserId = e.UserId 
                AND (b.CategoryId IS NULL OR b.CategoryId = e.CategoryId)
                AND FORMAT(e.ExpenseDate, 'yyyy-MM') = b.MonthYear
            WHERE b.UserId = @UserId AND b.MonthYear = @MonthYear
            GROUP BY b.BudgetId, b.CategoryId, c.CategoryName, b.BudgetAmount
            ORDER BY b.CategoryId ASC";

        /// <summary>
        /// GET /api/reports/monthly/{userId} - Monthly report
        /// Parameters: @UserId, @Month, @Year
        /// </summary>
        public const string GetMonthlyReport = @"
            SELECT 
                u.UserId, u.FullName AS UserFullName,
                @Month AS Month, @Year AS Year,
                FORMAT(DATEFROMPARTS(@Year, @Month, 1), 'yyyy-MM') AS MonthYear,
                ISNULL(SUM(e.Amount), 0) AS TotalExpenses,
                COUNT(e.ExpenseId) AS TotalExpenseCount
            FROM Users u
            LEFT JOIN Expenses e ON u.UserId = e.UserId 
                AND MONTH(e.ExpenseDate) = @Month 
                AND YEAR(e.ExpenseDate) = @Year
            WHERE u.UserId = @UserId
            GROUP BY u.UserId, u.FullName";

        /// <summary>
        /// Get top expenses for monthly report
        /// Parameters: @UserId, @Month, @Year, @TopN
        /// </summary>
        public const string GetTopExpensesForMonth = @"
            SELECT TOP (@TopN)
                e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.ExpenseDate, e.Description, 
                e.PaymentMethod, e.Status, e.CreatedAt, e.UpdatedAt,
                c.CategoryName, u.FullName AS UserFullName
            FROM Expenses e
            INNER JOIN Categories c ON e.CategoryId = c.CategoryId
            INNER JOIN Users u ON e.UserId = u.UserId
            WHERE e.UserId = @UserId 
            AND MONTH(e.ExpenseDate) = @Month 
            AND YEAR(e.ExpenseDate) = @Year
            ORDER BY e.Amount DESC";
    }
}