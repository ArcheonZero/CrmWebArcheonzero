using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface IClientRepository
    {
        Task<List<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task AddAsync(Client client, int userId);
        Task UpdateAsync(Client client, int userId);
        Task SoftDeleteAsync(int id, int userId);
        Task RestoreAsync(int id, int userId);
        Task PermanentDeleteAsync(int id);
        Task<List<Client>> GetDeletedAsync();
        Task<List<Client>> SearchAsync(string query);
        Task<Dictionary<string, int>> GetStatisticsAsync();
        Task<Client?> GetByPhoneOrEmailAsync(string? phone, string? email);
        Task<Client?> GetByPhoneAsync(string? phone);
        Task<Client?> GetByEmailAsync(string? email);
    }
}