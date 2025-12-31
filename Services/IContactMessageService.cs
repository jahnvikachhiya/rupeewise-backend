using ExpenseManagementAPI.DTOs;

namespace ExpenseManagementAPI.Services
{
    public interface IContactMessageService
    {
        Task<(bool success, string message, long? id)> CreateMessageAsync(CreateContactMessageRequest request);
        Task<(bool success, string message, List<ContactMessageDTO>? data)> GetAllMessagesAsync();
    }
}



