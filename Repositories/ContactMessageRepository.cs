using ExpenseManagementAPI.Data;
using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    public class ContactMessageRepository : IContactMessageRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public ContactMessageRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<long> CreateMessageAsync(ContactMessage message)
        {
            var query = @"
                INSERT INTO contact_messages (name, email, subject, message, created_at)
                VALUES (@Name, @Email, @Subject, @Message, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            var parameters = new Dictionary<string, object>
            {
                { "@Name", message.Name },
                { "@Email", message.Email },
                { "@Subject", message.Subject ?? (object)DBNull.Value },
                { "@Message", message.Message },
                { "@CreatedAt", message.CreatedAt }
            };

            return await Task.Run(() => (long)_dbHelper.ExecuteInsertWithId(query, parameters));
        }

        public async Task<List<ContactMessage>> GetAllMessagesAsync()
        {
            var query = @"
                SELECT id, name, email, subject, message, created_at
                FROM contact_messages
                ORDER BY created_at DESC";

            return await Task.Run(() =>
            {
                var messages = new List<ContactMessage>();
                using var reader = _dbHelper.ExecuteReader(query);
                while (reader.Read())
                {
                    messages.Add(new ContactMessage
                    {
                        Id = reader.GetInt64(reader.GetOrdinal("id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        Subject = reader.IsDBNull(reader.GetOrdinal("subject")) ? null : reader.GetString(reader.GetOrdinal("subject")),
                        Message = reader.GetString(reader.GetOrdinal("message")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                    });
                }
                return messages;
            });
        }

        public async Task<ContactMessage?> GetMessageByIdAsync(long id)
        {
            var query = @"
                SELECT id, name, email, subject, message, created_at
                FROM contact_messages
                WHERE id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            return await Task.Run(() =>
            {
                using var reader = _dbHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    return new ContactMessage
                    {
                        Id = reader.GetInt64(reader.GetOrdinal("id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        Subject = reader.IsDBNull(reader.GetOrdinal("subject")) ? null : reader.GetString(reader.GetOrdinal("subject")),
                        Message = reader.GetString(reader.GetOrdinal("message")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                    };
                }
                return null;
            });
        }

        public async Task<bool> DeleteMessageAsync(long id)
        {
            var query = "DELETE FROM contact_messages WHERE id = @Id";
            var parameters = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            return await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters) > 0);
        }
    }
}