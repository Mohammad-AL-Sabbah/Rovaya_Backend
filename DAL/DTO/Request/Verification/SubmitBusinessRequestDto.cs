using Microsoft.AspNetCore.Http;
using Rovaya.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request
{
    public class SubmitBusinessRequestDto
    {
        [Required, MaxLength(150)]
        public string BusinessName { get; set; } = null!;

        [Required]
        public BusinessCategory Category { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? WorkingHours { get; set; }

        // استقبال الصور مباشرة في نفس الطلب
        public List<IFormFile>? Images { get; set; }
    }

    public class ReviewBusinessRequestDto
    {
        [Required]
        public VerificationStatus NewStatus { get; set; } // Verified or Rejected

        [MaxLength(500)]
        public string? AdminNotes { get; set; } // مطلوب في حال الرفض
    }
}