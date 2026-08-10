using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly IAuthService _authService;

        public ChatController(IChatRepository chatRepository, IAuthService authService)
        {
            _chatRepository = chatRepository;
            _authService = authService;
        }
        public async Task<IActionResult> Index()
        {
            var messages = await _chatRepository.GetMessagesAsync();
            return View(messages);
        }
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return RedirectToAction(nameof(Index));

            var userId = GetCurrentUserId();
            var user = await _authService.GetUserByIdAsync(userId);

            var chatMessage = new ChatMessage
            {
                UserId = userId,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            await _chatRepository.AddMessageAsync(chatMessage);
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