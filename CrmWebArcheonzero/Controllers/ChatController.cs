using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IClientRepository _clientRepository;

        public ChatController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _clientRepository.GetChatMessagesAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return RedirectToAction(nameof(Index));

            var userId = GetCurrentUserId();
            var user = await _clientRepository.GetUserByIdAsync(userId);

            var chatMessage = new ChatMessage
            {
                UserId = userId,
                Message = message,
                SentAt = DateTime.Now
            };

            await _clientRepository.AddChatMessageAsync(chatMessage);
            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }
    }
}