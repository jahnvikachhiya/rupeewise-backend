using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.Repositories;
using ExpenseManagementAPI.Helpers;
using Microsoft.Extensions.Configuration;

namespace ExpenseManagementAPI.Seeders
{
    public class AdminSeeder
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;

        public AdminSeeder(IUserRepository userRepository, PasswordHasher passwordHasher, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            var adminConfig = _configuration.GetSection("AdminAccount");
            string adminEmail = adminConfig["Email"] ?? "admin@example.com";

            // Check if admin already exists
            var existingAdmin = await _userRepository.GetUserByUsernameOrEmailAsync(adminEmail);
            if (existingAdmin != null)
                return; // Admin already exists, no action

            // Create admin
            var admin = new User
            {
                Username = adminConfig["Username"] ?? "admin",
                Email = adminEmail,
                PasswordHash = _passwordHasher.HashPassword(adminConfig["Password"] ?? "Admin@123"),
                FullName = "System Administrator",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.CreateUserAsync(admin);
        }
    }
}
