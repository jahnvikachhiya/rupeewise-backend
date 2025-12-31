using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    public interface IContactMessageRepository
    {
        Task<long> CreateMessageAsync(ContactMessage message);
        Task<List<ContactMessage>> GetAllMessagesAsync();
        Task<ContactMessage?> GetMessageByIdAsync(long id);
        Task<bool> DeleteMessageAsync(long id);
    }
}