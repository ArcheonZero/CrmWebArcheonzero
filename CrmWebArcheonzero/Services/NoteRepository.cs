using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmWebArcheonzero.Services
{
    public class NoteRepository : INoteRepository
    {
        private readonly ApplicationDbContext _context;

        public NoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Note>> GetByClientAsync(int clientId)
        {
            return await _context.Notes
                .Where(n => n.ClientId == clientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            return await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task AddAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Note note)
        {
            var existing = await _context.Notes.FindAsync(note.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(note);
                _context.Entry(existing).Property(x => x.CreatedAt).IsModified = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }
    }
}