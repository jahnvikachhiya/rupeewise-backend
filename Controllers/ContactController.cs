using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseManagementAPI.Models;
using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Services;

namespace ExpenseManagementAPI.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly IContactMessageService _contactService;

        public ContactController(IContactMessageService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateMessage([FromBody] CreateContactMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", 400));
            }

            var (success, message, id) = await _contactService.CreateMessageAsync(request);

            if (success)
            {
                return Ok(ApiResponse<object>.SuccessResponse(new { id }, message, 201));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMessages()
        {
            var (success, message, data) = await _contactService.GetAllMessagesAsync();

            if (success)
            {
                return Ok(ApiResponse<List<ContactMessageDTO>>.SuccessResponse(data!, message));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse(message, 400));
        }
    }
}