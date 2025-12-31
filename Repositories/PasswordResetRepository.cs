using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public PasswordResetRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<int> CreateTokenAsync(PasswordResetToken token)
        {
            var query = @"
                INSERT INTO password_reset_tokens (user_id, token, expires_at, used, created_at)
                VALUES (@UserId, @Token, @ExpiresAt, @Used, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", token.UserId },
                { "@Token", token.Token },
                { "@ExpiresAt", token.ExpiresAt },
                { "@Used", token.Used },
                { "@CreatedAt", token.CreatedAt }
            };

            return await Task.Run(() => _dbHelper.ExecuteInsertWithId(query, parameters));
        }

   public async Task<PasswordResetToken?> GetValidTokenAsync(int userId, string otp)
{
    var query = @"
        SELECT TOP 1 id, user_id, token, expires_at, used, created_at
        FROM password_reset_tokens
        WHERE user_id = @UserId
          AND token = @OTP
          AND used = 0
          AND expires_at > GETDATE()
        ORDER BY created_at DESC";

    var parameters = new Dictionary<string, object>
    {
        { "@UserId", userId },
        { "@OTP", otp }
    };

    return await Task.Run(() =>
    {
        using var reader = _dbHelper.ExecuteReader(query, parameters);
        if (reader.Read())
        {
            return new PasswordResetToken
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                Token = reader.GetString(reader.GetOrdinal("token")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at")),
                Used = reader.GetBoolean(reader.GetOrdinal("used")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            };
        }
        return null;
    });
}

        public async Task<bool> MarkTokenAsUsedAsync(int tokenId)
        {
            var query = "UPDATE password_reset_tokens SET used = 1 WHERE id = @TokenId";
            var parameters = new Dictionary<string, object>
            {
                { "@TokenId", tokenId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters) > 0);
        }

        public async Task<bool> DeleteExpiredTokensAsync(int userId)
        {
            var query = @"
                DELETE FROM password_reset_tokens 
                WHERE user_id = @UserId 
                  AND (used = 1 OR expires_at < SYSDATETIME())";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters) >= 0);
        }
    }
}