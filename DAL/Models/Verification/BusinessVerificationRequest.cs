using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rovaya.DAL.Models.Business
{
    /// <summary>
    /// طلب توثيق حساب المستخدم كصاحب عمل تجاري
    /// </summary>
    public class VerificationRequest
    {
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(150)]
        public string RequestTitle { get; set; } = null!;       // عنوان الطلب

        [Required, MaxLength(2000)]
        public string Description { get; set; } = null!;        // وصف النشاط

        [Required, MaxLength(50)]
        public string ContactPhone { get; set; } = null!;       // رقم التواصل

        [MaxLength(150)]
        public string? ContactEmail { get; set; }

        [MaxLength(100)]
        public string? BusinessName { get; set; }               // اسم النشاط (اختياري)

        [MaxLength(200)]
        public string? BusinessAddress { get; set; }            // عنوان النشاط (اختياري)

        public VerificationRequestStatus Status { get; set; } = VerificationRequestStatus.Pending;

        [MaxLength(500)]
        public string? AdminNotes { get; set; }                 // سبب الرفض / ملاحظة الموافقة

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByAdminId { get; set; }
    }
}