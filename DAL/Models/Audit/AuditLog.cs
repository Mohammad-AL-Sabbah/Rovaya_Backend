using Rovaya.DAL.Models.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rovaya.DAL.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = null!; // Block, Unblock, CreateEmployee, DeleteUser, CreateNews, UpdateNews, DeleteNews

        [Required]
        [ForeignKey(nameof(PerformedByUser))]
        public string PerformedByUserId { get; set; } = null!;
        public ApplicationUser? PerformedByUser { get; set; }

        [Required]
        [MaxLength(200)]
        public string PerformedByUserName { get; set; } = null!;

        public string? TargetUserId { get; set; }

        [MaxLength(200)]
        public string? TargetUserName { get; set; }

        [MaxLength(500)]
        public string? Details { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}