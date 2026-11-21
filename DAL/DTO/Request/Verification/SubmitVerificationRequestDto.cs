using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Business
{
    public class SubmitVerificationRequestDto
    {
        [Required(ErrorMessage = "عنوان الطلب مطلوب")]
        [MaxLength(150)]
        public string RequestTitle { get; set; } = null!;

        [Required(ErrorMessage = "الوصف مطلوب")]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "رقم التواصل مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        [MaxLength(50)]
        public string ContactPhone { get; set; } = null!;

        [EmailAddress]
        [MaxLength(150)]
        public string? ContactEmail { get; set; }

        [MaxLength(100)]
        public string? BusinessName { get; set; }

        [MaxLength(200)]
        public string? BusinessAddress { get; set; }
    }

    public class ReviewVerificationRequestDto
    {
        [Required]
        public bool Approve { get; set; }

        /// <summary>
        /// إلزامي في حال الرفض، اختياري في حال الموافقة
        /// </summary>
        [MaxLength(500)]
        public string? AdminNotes { get; set; }
    }
}