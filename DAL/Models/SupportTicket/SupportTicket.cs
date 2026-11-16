using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rovaya.DAL.Models.Support
{
    public class SupportTicket
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!; 
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required, MaxLength(2000)]
        public string Description { get; set; } = null!;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = null!;

        // سنحفظ روابط الصور كنص JSON (مثال: ["img1.jpg", "img2.jpg"])
        [MaxLength(2000)]
        public string? ImageUrlsJson { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.Sent;

        [MaxLength(1000)]
        public string? AdminNotes { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public string CreatedByUserId { get; set; } = null!;
        public ApplicationUser CreatedByUser { get; set; } = null!;

        [ForeignKey(nameof(UpdatedByUser))]
        public string? UpdatedByUserId { get; set; }
        public ApplicationUser? UpdatedByUser { get; set; }
    }
}