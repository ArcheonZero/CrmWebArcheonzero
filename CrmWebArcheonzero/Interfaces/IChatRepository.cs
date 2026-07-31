using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface IChatRepository
    {
        Task<List<ChatMessage>> GetMessagesAsync();
        Task AddMessageAsync(ChatMessage message);
    }
}