using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Services
{
    public interface IClientRepository
    {
        Task<Client?> GetByPhoneOrEmailAsync(string? phone, string? email);
        Task<List<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task SoftDeleteAsync(int id, int userId);
        Task RestoreAsync(int id);
        Task PermanentDeleteAsync(int id);
        Task<List<Client>> GetDeletedAsync();
        Task<List<Client>> SearchAsync(string query);
        Task<Dictionary<string, int>> GetStatisticsAsync();
        Task<List<ClientTask>> GetTasksByClientAsync(int clientId);
        Task AddTaskAsync(ClientTask task);
        Task ToggleTaskCompletionAsync(int taskId);
        Task<List<Note>> GetNotesByClientAsync(int clientId);
        Task AddNoteAsync(Note note);
        Task DeleteNoteAsync(int noteId);
        Task<List<Interaction>> GetInteractionsByClientAsync(int clientId);
        Task AddInteractionAsync(Interaction interaction);
        Task DeleteInteractionAsync(int interactionId);
		Task<ClientTask?> GetTaskByIdAsync(int id);
		Task UpdateTaskAsync(ClientTask task);
		Task DeleteTaskAsync(int id);
        Task<Note?> GetNoteByIdAsync(int id);
        Task UpdateNoteAsync(Note note);

        Task<Interaction?> GetInteractionByIdAsync(int id);
        Task UpdateInteractionAsync(Interaction interaction);
        Task<List<ChatMessage>> GetChatMessagesAsync();
        Task AddChatMessageAsync(ChatMessage message);
        Task<User?> GetUserByIdAsync(int id);

    }
}