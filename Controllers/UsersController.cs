using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.Services;
using System.Security.Claims;
using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UsersController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        /// <summary>
        /// POST /api/users/register - Register new user
        /// Access: Public
        /// Email-based admin assignment: If email in AdminEmails, Role = 'Admin'
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
    Console.WriteLine("=== REGISTER ENDPOINT HIT ===");
     Console.WriteLine($"Request object: {(request == null ? "NULL" : "NOT NULL")}");
    Console.WriteLine($"Username: '{request?.Username}'");
    Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400, 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

          //  var adminEmails = _configuration.GetSection("AdminEmails").Get<List<string>>() ?? new List<string>();
           // var (success, message, data) = await _userService.RegisterUserAsync(request, adminEmails);
    var adminEmails = _configuration.GetSection("AdminEmails").Get<List<string>>() ?? new List<string>();
    var (success, message, data) = await _userService.RegisterUserAsync(request!, adminEmails); 
            if (success)
            {
                return Ok(ApiResponse<RegisterResponse>.SuccessResponse(data!, message, 201));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// POST /api/users/login - User login
        /// Access: Public
        /// Returns JWT token
        /// </summary>
[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login([FromBody] LoginDTO request)
{
    // Validate that at least username or email is provided
    if (string.IsNullOrEmpty(request.Username) && string.IsNullOrEmpty(request.Email))
    {
        return BadRequest(ApiResponse<object>.ErrorResponse("Username or Email is required", 400));
    }

    // Map DTO to Model for service
    var loginRequest = new LoginRequest
    {
        UsernameOrEmail = !string.IsNullOrEmpty(request.Username) ? request.Username : request.Email ?? string.Empty,

        Password = request.Password
    };

    // Call the service
    var (success, message, data) = await _userService.LoginUserAsync(loginRequest);

    if (success)
        return Ok(ApiResponse<LoginResponse>.SuccessResponse(data!, message));

    return Unauthorized(ApiResponse<object>.ErrorResponse(message, 401));
}


        /// <summary>
        /// GET /api/users/profile/{userId} - Get user profile
        /// Access: Authenticated User (Own Profile)
        /// </summary>
        [HttpGet("profile/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            // Users can only view their own profile (unless Admin)
            if (requestingUserId != userId && userRole != "Admin")
            {
                return Forbid();
            }

            var (success, message, data) = await _userService.GetUserProfileAsync(userId);

            if (success)
            {
                return Ok(ApiResponse<UserDTO>.SuccessResponse(data!, message));
            }

            return NotFound(ApiResponse<object>.ErrorResponse(message, 404));
        }

        /// <summary>
        /// PUT /api/users/profile/{userId} - Update user profile
        /// Access: Authenticated User (Own Profile)
        /// </summary>
        [HttpPut("profile/{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only update their own profile
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message) = await _userService.UpdateUserProfileAsync(userId, request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// PUT /api/users/change-password/{userId} - Change password
        /// Access: Authenticated User (Own Account)
        /// </summary>
        [HttpPut("change-password/{userId}")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Users can only change their own password
            if (requestingUserId != userId)
            {
                return Forbid();
            }

            var (success, message) = await _userService.ChangePasswordAsync(userId, request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// GET /api/users - Get all users
        /// Access: Admin Only
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var (success, message, data) = await _userService.GetAllUsersAsync();

            if (success)
            {
                return Ok(ApiResponse<List<UserDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        /// <summary>
        /// PUT /api/users/deactivate/{userId} - Deactivate user
        /// Access: Admin Only
        /// </summary>
        [HttpPut("deactivate/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var (success, message) = await _userService.DeactivateUserAsync(userId);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(null, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));

        

        }

        //user again activation
        [HttpPut("activate/{userId}")]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> ActivateUser(int userId)
            {
                var (success, message) = await _userService.ActivateUserAsync(userId);

                if (success)
                    return Ok(ApiResponse<object>.SuccessResponse(null, message));

                return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
            }

    }
}

