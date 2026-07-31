using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface IHistoryRepository
    {
        Task<List<AssignmentHistory>> GetByClientAsync(int clientId);
        Task AddEntryAsync(AssignmentHistory entry);
    }
}