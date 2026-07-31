using System;
using System.Text.Json;

namespace CrmWebArcheonzero.Models
{
    public class AssignmentHistory
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public int AssignedByUserId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Новые поля для полной истории изменений
        public string ChangeType { get; set; } = string.Empty; // "Created", "Updated", "Deleted", "Restored", "Assigned"
        public string? FieldName { get; set; } // Какое поле изменилось (например, "Name", "Phone", "Status")
        public string? OldValue { get; set; }  // Старое значение
        public string? NewValue { get; set; }  // Новое значение

        // Навигационные свойства
        public Client? Client { get; set; }
        public User? FromUser { get; set; }
        public User? ToUser { get; set; }
        public User? AssignedByUser { get; set; }
    }
}