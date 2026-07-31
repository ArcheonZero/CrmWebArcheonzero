using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmWebArcheonzero.Services
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessage>> GetMessagesAsync()
        {
            return await _context.ChatMessages
                .Include(m => m.User)
                .OrderByDescending(m => m.SentAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            message.SentAt = DateTime.UtcNow;
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }
    }
}