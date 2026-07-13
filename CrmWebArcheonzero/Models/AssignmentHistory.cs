using System;

namespace CrmWebArcheonzero.Models
{
    public class AssignmentHistory
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? FromUserId { get; set; }
        public int ToUserId { get; set; }
        public int AssignedByUserId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.Now;

        public Client? Client { get; set; }
        public User? FromUser { get; set; }
        public User? ToUser { get; set; }
        public User? AssignedByUser { get; set; }
    }
}