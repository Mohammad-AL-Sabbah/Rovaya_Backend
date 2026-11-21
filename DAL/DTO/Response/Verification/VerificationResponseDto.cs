using Rovaya.DAL.Enums;

namespace Rovaya.DAL.DTO.Response.Business
{
    public class UserVerificationStatusResponse
    {
        public string UserId { get; set; } = null!;
        public UserVerificationStatus Status { get; set; }
        public string StatusLabel { get; set; } = null!;
        public bool CanAddPlaces { get; set; }
        public string? Notes { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public VerificationRequestDto? ActiveRequest { get; set; }
    }

    public class VerificationRequestDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string RequestTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessAddress { get; set; }
        public VerificationRequestStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // ✅ يجب أن يكون هذا السطر موجوداً
        public string? ReviewedByAdminName { get; set; }
    }
}