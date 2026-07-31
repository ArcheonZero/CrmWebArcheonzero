using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Models;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<ClientTask?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateTaskAsync(ClientTask task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Client?> GetByPhoneOrEmailAsync(string? phone, string? email)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c =>
                    (phone != null && c.Phone == phone) ||
                    (email != null && c.Email == email)
                );
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

            // Запись в историю
            var historyEntry = new AssignmentHistory
            {
                ClientId = client.Id,
                ChangeType = "Created",
                FieldName = "Client",
                OldValue = null,
                NewValue = client.Name,
                AssignedByUserId = userId,
                AssignedAt = DateTime.UtcNow
            };
            _context.AssignmentHistories.Add(historyEntry);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client, int userId)
        {
            var existing = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == client.Id);

            if (existing == null)
                return;

            // Сравниваем поля и создаём записи истории
            var historyEntries = new List<AssignmentHistory>();

            if (existing.Name != client.Name)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Name", existing.Name, client.Name, userId));
            if (existing.Phone != client.Phone)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Phone", existing.Phone, client.Phone, userId));
            if (existing.Email != client.Email)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Email", existing.Email, client.Email, userId));
            if (existing.Company != client.Company)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Company", existing.Company, client.Company, userId));
            if (existing.Status != client.Status)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Status", existing.Status, client.Status, userId));
            if (existing.Source != client.Source)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Source", existing.Source, client.Source, userId));
            if (existing.Tags != client.Tags)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Tags", existing.Tags, client.Tags, userId));
            if (existing.Notes != client.Notes)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Notes", existing.Notes, client.Notes, userId));
            if (existing.Birthday != client.Birthday)
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Birthday", existing.Birthday, client.Birthday, userId));

            // Обновляем клиента
            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Сохраняем историю
            if (historyEntries.Any())
            {
                _context.AssignmentHistories.AddRange(historyEntries);
                await _context.SaveChangesAsync();
            }
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

                // Запись в историю
                var historyEntry = new AssignmentHistory
                {
                    ClientId = id,
                    ChangeType = "Deleted",
                    FieldName = "Client",
                    OldValue = client.Name,
                    NewValue = null,
                    AssignedByUserId = userId,
                    AssignedAt = DateTime.UtcNow
                };
                _context.AssignmentHistories.Add(historyEntry);
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

                // Запись в историю
                var historyEntry = new AssignmentHistory
                {
                    ClientId = id,
                    ChangeType = "Restored",
                    FieldName = "Client",
                    OldValue = null,
                    NewValue = client.Name,
                    AssignedByUserId = userId,
                    AssignedAt = DateTime.UtcNow
                };
                _context.AssignmentHistories.Add(historyEntry);
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

        public async Task<List<ClientTask>> GetTasksByClientAsync(int clientId)
        {
            return await _context.Tasks
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task AddTaskAsync(ClientTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleTaskCompletionAsync(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Note>> GetNotesByClientAsync(int clientId)
        {
            return await _context.Notes
                .Where(n => n.ClientId == clientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task AddNoteAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int noteId)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Interaction>> GetInteractionsByClientAsync(int clientId)
        {
            return await _context.Interactions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task AddInteractionAsync(Interaction interaction)
        {
            _context.Interactions.Add(interaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteInteractionAsync(int interactionId)
        {
            var interaction = await _context.Interactions.FindAsync(interactionId);
            if (interaction != null)
            {
                _context.Interactions.Remove(interaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task UpdateNoteAsync(Note note)
        {
            var existing = await _context.Notes.FindAsync(note.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(note);
                _context.Entry(existing).Property(x => x.CreatedAt).IsModified = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Interaction?> GetInteractionByIdAsync(int id)
        {
            return await _context.Interactions.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateInteractionAsync(Interaction interaction)
        {
            _context.Interactions.Update(interaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync()
        {
            return await _context.ChatMessages
                .Include(m => m.User)
                .OrderByDescending(m => m.SentAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task AddChatMessageAsync(ChatMessage message)
        {
            message.SentAt = DateTime.UtcNow;
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        // ============================================================
        // ИСТОРИЯ ИЗМЕНЕНИЙ
        // ============================================================

        public async Task<List<AssignmentHistory>> GetHistoryByClientAsync(int clientId)
        {
            return await _context.AssignmentHistories
                .Where(h => h.ClientId == clientId)
                .Include(h => h.FromUser)
                .Include(h => h.ToUser)
                .Include(h => h.AssignedByUser)
                .OrderByDescending(h => h.AssignedAt)
                .ToListAsync();
        }

        public async Task AddHistoryEntryAsync(AssignmentHistory entry)
        {
            _context.AssignmentHistories.Add(entry);
            await _context.SaveChangesAsync();
        }

        private AssignmentHistory CreateHistoryEntry(int clientId, string changeType, string fieldName, object? oldValue, object? newValue, int userId)
        {
            return new AssignmentHistory
            {
                ClientId = clientId,
                ChangeType = changeType,
                FieldName = fieldName,
                OldValue = oldValue?.ToString(),
                NewValue = newValue?.ToString(),
                AssignedByUserId = userId,
                AssignedAt = DateTime.UtcNow
            };
        }
    }
}