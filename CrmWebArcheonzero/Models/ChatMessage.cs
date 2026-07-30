using System;

namespace CrmWebArcheonzero.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}