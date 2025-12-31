using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Repositories;
using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly IContactMessageRepository _contactRepository;

        public ContactMessageService(IContactMessageRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<(bool success, string message, long? id)> CreateMessageAsync(CreateContactMessageRequest request)
        {
            try
            {
                var contactMessage = new ContactMessage
                {
                    Name = request.Name,
                    Email = request.Email,
                    Subject = request.Subject,
                    Message = request.Message,
                    CreatedAt = DateTime.UtcNow
                };

                var id = await _contactRepository.CreateMessageAsync(contactMessage);

                return (true, "Message sent successfully", id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateMessageAsync: {ex.Message}");
                return (false, "Failed to send message", null);
            }
        }

        public async Task<(bool success, string message, List<ContactMessageDTO>? data)> GetAllMessagesAsync()
        {
            try
            {
                var messages = await _contactRepository.GetAllMessagesAsync();

                var dtos = messages.Select(m => new ContactMessageDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Subject = m.Subject,
                    Message = m.Message,
                    CreatedAt = m.CreatedAt
                }).ToList();

                return (true, "Messages retrieved successfully", dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllMessagesAsync: {ex.Message}");
                return (false, "Failed to retrieve messages", null);
            }
        }
    }
}