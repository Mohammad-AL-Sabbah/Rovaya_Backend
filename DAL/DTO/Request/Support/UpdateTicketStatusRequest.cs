using Rovaya.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Support
{
    public class UpdateTicketStatusRequest
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public TicketStatus NewStatus { get; set; }

        [MaxLength(1000)]
        public string? AdminNotes { get; set; }
    }
}