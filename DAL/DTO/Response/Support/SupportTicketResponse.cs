using Rovaya.DAL.Enums;

namespace Rovaya.DAL.DTO.Response.Support
{
    public class SupportTicketResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string UserPhone { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string> ImageUrls { get; set; } = new();
        public TicketStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}