using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    /// <summary>
    /// User Repository Implementation
    /// Handles all user-related database operations using ADO.NET
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public UserRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// POST /api/users/register - Create new user
        /// </summary>
        public async Task<int> CreateUserAsync(User user)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Username", user.Username },
                { "@Email", user.Email },
                { "@PasswordHash", user.PasswordHash },
                { "@FullName", user.FullName ?? (object)DBNull.Value },
                { "@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value },
                { "@Role", user.Role }
            };

            return await Task.Run(() => _dbHelper.ExecuteInsertWithId(SqlQueryHelper.InsertUser, parameters));
        }

        /// <summary>
        /// POST /api/users/login - Get user by username or email
        /// </summary>
        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UsernameOrEmail", usernameOrEmail }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetUserByUsernameOrEmail, parameters);
                if (reader.Read())
                {
                    return new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        Role = reader.GetString(reader.GetOrdinal("Role")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }
                return null;
            });
        }

        /// <summary>
        /// GET /api/users/profile/{userId} - Get user by ID
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetUserById, parameters);
                if (reader.Read())
                {
                    return new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        Role = reader.GetString(reader.GetOrdinal("Role")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }
                return null;
            });
        }

        /// <summary>
        /// GET /api/users - Get all users (Admin only)
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await Task.Run(() =>
            {
                var users = new List<User>();
                using var reader = _dbHelper.ExecuteReader(SqlQueryHelper.GetAllUsers);
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        Role = reader.GetString(reader.GetOrdinal("Role")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }
                return users;
            });
        }

        /// <summary>
        /// PUT /api/users/profile/{userId} - Update user profile
        /// </summary>
        public async Task<bool> UpdateUserProfileAsync(int userId, string fullName, string email, string? phoneNumber)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@FullName", fullName },
                { "@Email", email },
                { "@PhoneNumber", phoneNumber ?? (object)DBNull.Value }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.UpdateUserProfile, parameters) > 0);
        }

        /// <summary>
        /// PUT /api/users/change-password/{userId} - Change password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string passwordHash)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@PasswordHash", passwordHash }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.ChangePassword, parameters) > 0);
        }

        /// <summary>
        /// PUT /api/users/deactivate/{userId} - Deactivate user (Admin only)
        /// </summary>
        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(SqlQueryHelper.DeactivateUser, parameters) > 0);
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            return await Task.Run(() => _dbHelper.RecordExists(SqlQueryHelper.CheckUsernameExists, parameters));
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Email", email }
            };

            return await Task.Run(() => _dbHelper.RecordExists(SqlQueryHelper.CheckEmailExists, parameters));
        }

        /// <summary>
        /// Check if email exists for other user (for update validation)
        /// </summary>
        public async Task<bool> EmailExistsForOtherUserAsync(string email, int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Email", email },
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.RecordExists(SqlQueryHelper.CheckEmailExistsForOtherUser, parameters));
        }

        //for user activation again
        public async Task<bool> ActivateUserAsync(int userId)
{
    var parameters = new Dictionary<string, object>
    {
        { "@UserId", userId }
    };

    return await Task.Run(() =>
        _dbHelper.ExecuteNonQuery(SqlQueryHelper.ActivateUser, parameters) > 0
    );
}


        
    }
}