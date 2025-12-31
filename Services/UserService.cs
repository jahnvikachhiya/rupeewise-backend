using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;
using ExpenseManagementAPI.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseManagementAPI.Services
{
    /// <summary>
    /// User Service Implementation
    /// Handles user management business logic
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, PasswordHasher passwordHasher, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        /// <summary>
        /// POST /api/users/register - Register new user
        /// Email-based admin assignment: If email in AdminEmails list, Role = 'Admin'
        /// </summary>
        public async Task<(bool success, string message, RegisterResponse? data)> RegisterUserAsync(RegisterRequest request, List<string> adminEmails)
        {
            try
            {
                // Validate username
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    return (false, "Username already exists", null);
                }

                // Validate email
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return (false, "Email already exists", null);
                }

                // Determine role based on email (Option B: Email-based admin)
                                // ❌ Block admin registration
                    if (adminEmails.Any(e => e.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        return (false, "Admin account cannot be registered", null);
                    }

                    // ✅ Always normal user
                    string role = "User";


                // Create user object
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Insert user into database
                int userId = await _userRepository.CreateUserAsync(user);

                if (userId > 0)
                {
                    var response = new RegisterResponse
                    {
                        UserId = userId,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        RegisteredAt = user.CreatedAt,
                        Message = "Registration successful! Please login to continue."
                    };

                    return (true, "User registered successfully", response);
                }

                return (false, "Failed to register user", null);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// POST /api/users/login - Authenticate user and generate JWT token
        /// </summary>
       public async Task<(bool success, string message, LoginResponse? data)> LoginUserAsync(LoginRequest request)
{
    var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail);

    if (user == null)
        return (false, "Invalid username/email or password", null);

    if (!user.IsActive)
        return (false, "Your account has been deactivated. Please contact administrator.", null);

    if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        return (false, "Invalid username/email or password", null);

    // ❌ NO ROLE MODIFICATION HERE

    string token = GenerateJwtToken(user);

    var expiryHours = int.Parse(
        _configuration.GetSection("JwtSettings")["ExpiryInHours"] ?? "24"
    );

    return (true, "Login successful", new LoginResponse
    {
        Token = token,
        TokenType = "Bearer",
        ExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
        User = new UserInfo
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName ?? string.Empty,
            Role = user.Role,
            IsActive = user.IsActive
        }
    });
}


        /// <summary>
        /// GET /api/users/profile/{userId} - Get user profile
        /// </summary>
        public async Task<(bool success, string message, UserDTO? data)> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return (false, "User not found", null);
                }

                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return (true, "User profile retrieved successfully", userDto);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// GET /api/users - Get all users (Admin only)
        /// </summary>
        public async Task<(bool success, string message, List<UserDTO>? data)> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();

                var userDtos = users.Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return (true, "Users retrieved successfully", userDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// PUT /api/users/profile/{userId} - Update user profile
        /// </summary>
        public async Task<(bool success, string message)> UpdateUserProfileAsync(int userId, UpdateProfileRequest request)
        {
            try
            {
                // Check if user exists
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                // Check if email is being changed and already exists for another user
                if (user.Email != request.Email)
                {
                    if (await _userRepository.EmailExistsForOtherUserAsync(request.Email, userId))
                    {
                        return (false, "Email already exists");
                    }
                }

                // Update profile
                bool updated = await _userRepository.UpdateUserProfileAsync(userId, request.FullName, request.Email, request.PhoneNumber);

                if (updated)
                {
                    return (true, "Profile updated successfully");
                }

                return (false, "Failed to update profile");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// PUT /api/users/change-password/{userId} - Change password
        /// </summary>
        public async Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                // Get user
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                // Verify old password
                if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
                {
                    return (false, "Old password is incorrect");
                }

                // Hash new password
                string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

                // Update password
                bool updated = await _userRepository.ChangePasswordAsync(userId, newPasswordHash);

                if (updated)
                {
                    return (true, "Password changed successfully");
                }

                return (false, "Failed to change password");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// PUT /api/users/deactivate/{userId} - Deactivate user (Admin only)
        /// </summary>
        public async Task<(bool success, string message)> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                bool deactivated = await _userRepository.DeactivateUserAsync(userId);

                if (deactivated)
                {
                    return (true, "User deactivated successfully");
                }

                return (false, "Failed to deactivate user");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
  public string GenerateJwtToken(User user)
{
    // Read from environment variable directly (same as Program.cs does)
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? throw new InvalidOperationException("JWT_SECRET_KEY not set");
    
    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
        ?? throw new InvalidOperationException("JWT_ISSUER not set");
    
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        ?? throw new InvalidOperationException("JWT_AUDIENCE not set");
    
    var jwtSettings = _configuration.GetSection("JwtSettings");
    var expiryHours = int.Parse(jwtSettings["ExpiryInHours"] ?? "24");

    // Decode Base64 secret key
    var keyBytes = Convert.FromBase64String(secretKey);
    var securityKey = new SymmetricSecurityKey(keyBytes);
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), 
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(expiryHours),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

        public async Task<(bool success, string message)> ActivateUserAsync(int userId)
{
    try
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return (false, "User not found");

        bool activated = await _userRepository.ActivateUserAsync(userId);

        if (activated)
            return (true, "User activated successfully");

        return (false, "Failed to activate user");
    }
    catch (Exception ex)
    {
        return (false, $"Error: {ex.Message}");
    }
}

    }
}