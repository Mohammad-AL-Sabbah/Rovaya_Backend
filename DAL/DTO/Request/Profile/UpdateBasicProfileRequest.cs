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
    }
}