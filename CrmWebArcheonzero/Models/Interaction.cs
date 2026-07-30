using System;

namespace CrmWebArcheonzero.Models
{
    public class Interaction
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "Call";
        public string Description { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;

        public Client? Client { get; set; }
    }
}