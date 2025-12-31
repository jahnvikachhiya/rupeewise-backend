using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ExpenseManagementAPI.Middleware
{
    /// <summary>
    /// Authentication Middleware (Optional)
    /// Provides additional JWT token validation and logging
    /// Note: Primary authentication is handled by ASP.NET Core JWT middleware in Program.cs
    /// This middleware adds extra validation and logging capabilities
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the endpoint requires authorization
            var endpoint = context.GetEndpoint();
            var requiresAuth = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

            if (requiresAuth)
            {
                // Get the Authorization header
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authHeader))
                {
                    _logger.LogWarning("Authorization header missing for protected endpoint: {Path}", context.Request.Path);
                }
                else if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();

                    // Log authentication attempt (without exposing token)
                    _logger.LogInformation("Authentication attempt for endpoint: {Path}", context.Request.Path);

                    // Additional token validation could be added here if needed
                    // The actual JWT validation is done by ASP.NET Core JWT middleware
                    
                    // Get user information from claims (after JWT middleware validates)
                    if (context.User?.Identity?.IsAuthenticated == true)
                    {
                        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                        var username = context.User.FindFirst(ClaimTypes.Name)?.Value;

                        _logger.LogInformation("User authenticated - UserId: {UserId}, Username: {Username}, Role: {Role}", 
                            userId, username, userRole);

                        // Add custom headers for debugging (optional)
                        context.Response.OnStarting(() =>
                        {
                            if (!context.Response.Headers.ContainsKey("X-Authenticated-User"))
                            {
                                context.Response.Headers.Append("X-Authenticated-User", username ?? "Unknown");
                            }
                            return Task.CompletedTask;
                        });
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid Authorization header format for endpoint: {Path}", context.Request.Path);
                }
            }

            // Continue to next middleware
            await _next(context);
        }
    }

    /// <summary>
    /// Extension method to register the middleware
    /// </summary>
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}