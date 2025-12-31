using System.Text.RegularExpressions;

namespace ExpenseManagementAPI.Helpers
{
    /// <summary>
    /// Validation Helper
    /// Provides validation utilities for user input
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate email format (real-world emails only, block example.com)
        /// Returns null if valid, otherwise an error message
        /// </summary>
        public static string? ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email cannot be empty.";

            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(email))
                return "Email format is invalid.";

            var blockedDomains = new[]
            {
                "@example.com",
                "@example.net",
                "@example.org",
                "@localhost"
            };

            if (blockedDomains.Any(d => email.EndsWith(d, StringComparison.OrdinalIgnoreCase)))
                return "Email domain is not allowed.";

            return null; // Valid
        }

        /// <summary>
        /// Validate username (industry-standard rules for financial apps)
        /// Returns null if valid, otherwise an error message
        /// </summary>
        public static string? ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Username cannot be empty.";

            if (username.Length < 6)
                return "Username must be at least 6 characters long.";

            if (username.Length > 30)
                return "Username cannot be more than 30 characters long.";

            // Must start and end with a letter or number
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9].*[a-zA-Z0-9]$"))
                return "Username must start and end with a letter or number.";

            // Allowed characters: letters, numbers, dot, hyphen, underscore
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9._-]+$"))
                return "Username can contain only letters, numbers, dots (.), hyphens (-), or underscores (_).";

            // No consecutive special characters
            if (Regex.IsMatch(username, @"[._-]{2,}"))
                return "Username cannot have consecutive dots, hyphens, or underscores.";

            // Block sensitive words
            var blockedWords = new[] { "admin", "support", "bank", "root" };
            foreach (var word in blockedWords)
            {
                if (username.ToLower().Contains(word))
                    return $"Username cannot contain '{word}'.";
            }

            return null; // Valid
        }

        /// <summary>
        /// Validate password strength
        /// Returns null if valid, otherwise an error message
        /// </summary>
        public static string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password cannot be empty.";

            if (password.Length < 6)
                return "Password must be at least 6 characters long.";

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return "Password must contain at least one uppercase letter.";

            if (!Regex.IsMatch(password, @"[a-z]"))
                return "Password must contain at least one lowercase letter.";

            if (!Regex.IsMatch(password, @"\d"))
                return "Password must contain at least one number.";

            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return "Password must contain at least one special character.";

            if (password.Contains(" "))
                return "Password cannot contain spaces.";

            return null; // Valid
        }

        /// <summary>
        /// Validate phone number format
        /// Returns null if valid, otherwise an error message
        /// </summary>
        public static string? ValidatePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null; // Optional field

            var regex = new Regex(@"^\+?[1-9]\d{1,14}$");
            if (!regex.IsMatch(phoneNumber))
                return "Phone number format is invalid. Use international format like +919876543210.";

            return null; // Valid
        }

        /// <summary>
        /// Validate MonthYear format (YYYY-MM)
        /// </summary>
        public static bool IsValidMonthYear(string monthYear)
        {
            if (string.IsNullOrWhiteSpace(monthYear))
                return false;

            var regex = new Regex(@"^\d{4}-(0[1-9]|1[0-2])$");
            return regex.IsMatch(monthYear);
        }

        /// <summary>
        /// Validate payment method
        /// </summary>
        public static bool IsValidPaymentMethod(string paymentMethod)
        {
            var validMethods = new[] { "Cash", "Card", "UPI", "Net Banking", "Others" };
            return validMethods.Contains(paymentMethod);
        }

        /// <summary>
        /// Validate expense amount
        /// </summary>
        public static bool IsValidAmount(decimal amount)
        {
            return amount > 0 && amount <= 10000000;
        }

        /// <summary>
        /// Validate budget amount
        /// </summary>
        public static bool IsValidBudgetAmount(decimal amount)
        {
            return amount > 0 && amount <= 100000000;
        }

        /// <summary>
        /// Validate hex color code
        /// </summary>
        public static bool IsValidColorCode(string? colorCode)
        {
            if (string.IsNullOrWhiteSpace(colorCode))
                return true; // Optional

            var regex = new Regex(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
            return regex.IsMatch(colorCode);
        }

        /// <summary>
        /// Sanitize string input (remove leading/trailing whitespace)
        /// </summary>
        public static string SanitizeString(string input)
        {
            return input?.Trim() ?? string.Empty;
        }
    }
}
