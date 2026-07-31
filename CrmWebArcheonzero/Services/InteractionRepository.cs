using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmWebArcheonzero.Services
{
    public class InteractionRepository : IInteractionRepository
    {
        private readonly ApplicationDbContext _context;

        public InteractionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Interaction>> GetByClientAsync(int clientId)
        {
            return await _context.Interactions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<Interaction?> GetByIdAsync(int id)
        {
            return await _context.Interactions.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task AddAsync(Interaction interaction)
        {
            _context.Interactions.Add(interaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Interaction interaction)
        {
            _context.Interactions.Update(interaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var interaction = await _context.Interactions.FindAsync(id);
            if (interaction != null)
            {
                _context.Interactions.Remove(interaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}