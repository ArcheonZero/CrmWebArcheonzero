using System;

namespace CrmWebArcheonzero.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Client? Client { get; set; }
    }
}