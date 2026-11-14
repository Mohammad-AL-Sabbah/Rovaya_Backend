using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Profile
{
    public class UpdateBasicProfileRequest
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "الدولة مطلوبة")]
        [MaxLength(100)]
        public string Country { get; set; } = null!;

        // صورة البروفايل (Base64 أو URL)
        public string? ProfileImageUrl { get; set; }

        // ✅ تمت إضافة رقم الهاتف
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = null!;

    }
}