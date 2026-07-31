using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmWebArcheonzero.Services
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public HistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AssignmentHistory>> GetByClientAsync(int clientId)
        {
            return await _context.AssignmentHistories
                .Where(h => h.ClientId == clientId)
                .Include(h => h.FromUser)
                .Include(h => h.ToUser)
                .Include(h => h.AssignedByUser)
                .OrderByDescending(h => h.AssignedAt)
                .ToListAsync();
        }

        public async Task AddEntryAsync(AssignmentHistory entry)
        {
            _context.AssignmentHistories.Add(entry);
            await _context.SaveChangesAsync();
        }
    }
}