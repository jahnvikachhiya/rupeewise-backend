namespace ExpenseManagementAPI.Models
{
    /// <summary>
    /// Standard API response wrapper for all endpoints
    /// Provides consistent JSON response format across entire API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Success response helper
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T? data, string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Error response helper
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors ?? new List<string> { message },
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}