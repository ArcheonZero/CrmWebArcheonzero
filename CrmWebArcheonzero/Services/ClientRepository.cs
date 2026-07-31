using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmWebArcheonzero.Services
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Client>> GetAllAsync()
        {
            return await _context.Clients
                .Where(c => !c.IsDeleted)
                .Include(c => c.AssignedUser)
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Client client, int userId)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client, int userId)
        {
            var existing = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == client.Id);

            if (existing == null)
                return;

            // Логика сравнения полей и создания истории будет в отдельном сервисе или репозитории
            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id, int userId)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                client.IsDeleted = true;
                client.DeletedAt = DateTime.UtcNow;
                client.DeletedByUserId = userId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(int id, int userId)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                client.IsDeleted = false;
                client.DeletedAt = null;
                client.DeletedByUserId = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task PermanentDeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Client>> GetDeletedAsync()
        {
            return await _context.Clients
                .Where(c => c.IsDeleted)
                .Include(c => c.AssignedUser)
                .ToListAsync();
        }

        public async Task<List<Client>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            var q = query.ToLower();
            return await _context.Clients
                .Where(c => !c.IsDeleted && (
                    c.Name.ToLower().Contains(q) ||
                    c.Phone.ToLower().Contains(q) ||
                    c.Email.ToLower().Contains(q) ||
                    c.Company.ToLower().Contains(q)))
                .Include(c => c.AssignedUser)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetStatisticsAsync()
        {
            var total = await _context.Clients.CountAsync(c => !c.IsDeleted);
            var active = await _context.Clients.CountAsync(c => c.Status == "Active" && !c.IsDeleted);
            var inactive = await _context.Clients.CountAsync(c => c.Status == "Inactive" && !c.IsDeleted);
            var lead = await _context.Clients.CountAsync(c => c.Status == "Lead" && !c.IsDeleted);

            return new Dictionary<string, int>
            {
                ["Total"] = total,
                ["Active"] = active,
                ["Inactive"] = inactive,
                ["Lead"] = lead
            };
        }

        public async Task<Client?> GetByPhoneAndEmailAsync(string? phone, string? email)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Phone == phone && c.Email == email);
        }

        public async Task<Client?> GetByPhoneAsync(string? phone)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<Client?> GetByEmailAsync(string? email)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == email);
        }
        public async Task<Client?> GetByPhoneOrEmailAsync(string? phone, string? email)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c =>
                    (phone != null && c.Phone == phone) ||
                    (email != null && c.Email == email)
                );
        }
    }
}