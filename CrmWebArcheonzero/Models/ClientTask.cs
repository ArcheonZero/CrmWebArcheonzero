using System;

namespace CrmWebArcheonzero.Models
{
    public class ClientTask
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = "Medium";
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Client? Client { get; set; }
    }
}