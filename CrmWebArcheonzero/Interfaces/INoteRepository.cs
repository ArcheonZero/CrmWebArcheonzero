using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface INoteRepository
    {
        Task<List<Note>> GetByClientAsync(int clientId);
        Task<Note?> GetByIdAsync(int id);
        Task AddAsync(Note note);
        Task UpdateAsync(Note note);
        Task DeleteAsync(int id);
    }
}