using CrmWebArcheonzero.Data;
using CrmWebArcheonzero.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CrmWebArcheonzero.Services
{
    public class ClientRepository : IClientRepository
    {
        private readonly Func<ApplicationDbContext> _contextFactory;

        public ClientRepository(Func<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Client?> GetByPhoneAndEmailAsync(string? phone, string? email)
        {
            using var context = _contextFactory();
            return await context.Clients
                .FirstOrDefaultAsync(c => c.Phone == phone && c.Email == email);
        }

        public async Task<Client?> GetByPhoneAsync(string? phone)
        {
            using var context = _contextFactory();
            return await context.Clients
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<Client?> GetByEmailAsync(string? email)
        {
            using var context = _contextFactory();
            return await context.Clients
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<ClientTask?> GetTaskByIdAsync(int id)
        {
            using var context = _contextFactory();
            return await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateTaskAsync(ClientTask task)
        {
            using var context = _contextFactory();
            context.Tasks.Update(task);
            await context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            using var context = _contextFactory();
            var task = await context.Tasks.FindAsync(id);
            if (task != null)
            {
                context.Tasks.Remove(task);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Client?> GetByPhoneOrEmailAsync(string? phone, string? email)
        {
            using var context = _contextFactory();
            return await context.Clients
                .FirstOrDefaultAsync(c =>
                    (phone != null && c.Phone == phone) ||
                    (email != null && c.Email == email)
                );
        }

        public async Task<List<Client>> GetAllAsync()
        {
            using var context = _contextFactory();
            return await context.Clients
                .Where(c => !c.IsDeleted)
                .Include(c => c.AssignedUser)
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            using var context = _contextFactory();
            return await context.Clients
                .Include(c => c.Interactions)
                .Include(c => c.Tasks)
                .Include(c => c.ClientNotes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Client client)
        {
            using var context = _contextFactory();
            context.Clients.Add(client);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client, int userId)
        {
            using var context = _contextFactory();

            // 1. Получаем старую версию клиента из базы (без отслеживания)
            var existing = await context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == client.Id);

            if (existing == null)
                return;

            // 2. Создаём список записей для истории
            var historyEntries = new List<AssignmentHistory>();

            // 3. Сравниваем поля
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
                historyEntries.Add(CreateHistoryEntry(client.Id, "Updated", "Birthday", existing.Birthday?.ToString("yyyy-MM-dd"), client.Birthday?.ToString("yyyy-MM-dd"), userId));

            // 4. Обновляем клиента
            context.Entry(client).State = EntityState.Modified;
            await context.SaveChangesAsync();

            // 5. Сохраняем историю, если есть изменения
            if (historyEntries.Any())
            {
                context.AssignmentHistories.AddRange(historyEntries);
                await context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id, int userId)
        {
            using var context = _contextFactory();
            var client = await context.Clients.FindAsync(id);
            if (client != null)
            {
                client.IsDeleted = true;
                client.DeletedAt = DateTime.UtcNow;
                client.DeletedByUserId = userId;
                await context.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(int id)
        {
            using var context = _contextFactory();
            var client = await context.Clients.FindAsync(id);
            if (client != null)
            {
                client.IsDeleted = false;
                client.DeletedAt = null;
                client.DeletedByUserId = null;
                await context.SaveChangesAsync();
            }
        }

        public async Task PermanentDeleteAsync(int id)
        {
            using var context = _contextFactory();
            var client = await context.Clients.FindAsync(id);
            if (client != null)
            {
                context.Clients.Remove(client);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Client>> GetDeletedAsync()
        {
            using var context = _contextFactory();
            return await context.Clients
                .Where(c => c.IsDeleted)
                .Include(c => c.AssignedUser)
                .ToListAsync();
        }

        public async Task<List<Client>> SearchAsync(string query)
        {
            using var context = _contextFactory();
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            var q = query.ToLower();
            return await context.Clients
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
            using var context = _contextFactory();
            var total = await context.Clients.CountAsync(c => !c.IsDeleted);
            var active = await context.Clients.CountAsync(c => c.Status == "Active" && !c.IsDeleted);
            var inactive = await context.Clients.CountAsync(c => c.Status == "Inactive" && !c.IsDeleted);
            var lead = await context.Clients.CountAsync(c => c.Status == "Lead" && !c.IsDeleted);

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
            using var context = _contextFactory();
            return await context.Tasks
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task AddTaskAsync(ClientTask task)
        {
            using var context = _contextFactory();
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
        }

        public async Task ToggleTaskCompletionAsync(int taskId)
        {
            using var context = _contextFactory();
            var task = await context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Note>> GetNotesByClientAsync(int clientId)
        {
            using var context = _contextFactory();
            return await context.Notes
                .Where(n => n.ClientId == clientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task AddNoteAsync(Note note)
        {
            using var context = _contextFactory();
            context.Notes.Add(note);
            await context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int noteId)
        {
            using var context = _contextFactory();
            var note = await context.Notes.FindAsync(noteId);
            if (note != null)
            {
                context.Notes.Remove(note);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Interaction>> GetInteractionsByClientAsync(int clientId)
        {
            using var context = _contextFactory();
            return await context.Interactions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task AddInteractionAsync(Interaction interaction)
        {
            using var context = _contextFactory();
            context.Interactions.Add(interaction);
            await context.SaveChangesAsync();
        }

        public async Task DeleteInteractionAsync(int interactionId)
        {
            using var context = _contextFactory();
            var interaction = await context.Interactions.FindAsync(interactionId);
            if (interaction != null)
            {
                context.Interactions.Remove(interaction);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            using var context = _contextFactory();
            return await context.Notes.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task UpdateNoteAsync(Note note)
        {
            using var context = _contextFactory();
            var existing = await context.Notes.FindAsync(note.Id);
            if (existing != null)
            {
                context.Entry(existing).CurrentValues.SetValues(note);
                context.Entry(existing).Property(x => x.CreatedAt).IsModified = false;
                await context.SaveChangesAsync();
            }
        }

        public async Task<Interaction?> GetInteractionByIdAsync(int id)
        {
            using var context = _contextFactory();
            return await context.Interactions.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateInteractionAsync(Interaction interaction)
        {
            using var context = _contextFactory();
            context.Interactions.Update(interaction);
            await context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync()
        {
            using var context = _contextFactory();
            return await context.ChatMessages
                .Include(m => m.User)
                .OrderByDescending(m => m.SentAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task AddChatMessageAsync(ChatMessage message)
        {
            using var context = _contextFactory();
            message.SentAt = DateTime.UtcNow;
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var context = _contextFactory();
            return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        // ============================================================
        // ИСТОРИЯ ИЗМЕНЕНИЙ
        // ============================================================

        public async Task<List<AssignmentHistory>> GetHistoryByClientAsync(int clientId)
        {
            using var context = _contextFactory();
            return await context.AssignmentHistories
                .Where(h => h.ClientId == clientId)
                .Include(h => h.FromUser)
                .Include(h => h.ToUser)
                .Include(h => h.AssignedByUser)
                .OrderByDescending(h => h.AssignedAt)
                .ToListAsync();
        }

        public async Task AddHistoryEntryAsync(AssignmentHistory entry)
        {
            using var context = _contextFactory();
            context.AssignmentHistories.Add(entry);
            await context.SaveChangesAsync();
        }

        private AssignmentHistory CreateHistoryEntry(int clientId, string changeType, string fieldName, string? oldValue, string? newValue, int userId)
        {
            return new AssignmentHistory
            {
                ClientId = clientId,
                ChangeType = changeType,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                AssignedByUserId = userId,
                AssignedAt = DateTime.UtcNow,
                FromUserId = null,
                ToUserId = null
            };
        }
    }
}