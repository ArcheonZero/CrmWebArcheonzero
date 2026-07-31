using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<ClientTask>> GetByClientAsync(int clientId);
        Task<ClientTask?> GetByIdAsync(int id);
        Task AddAsync(ClientTask task);
        Task UpdateAsync(ClientTask task);
        Task DeleteAsync(int id);
        Task ToggleCompletionAsync(int taskId);
    }
}