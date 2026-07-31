using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface IInteractionRepository
    {
        Task<List<Interaction>> GetByClientAsync(int clientId);
        Task<Interaction?> GetByIdAsync(int id);
        Task AddAsync(Interaction interaction);
        Task UpdateAsync(Interaction interaction);
        Task DeleteAsync(int id);
    }
}