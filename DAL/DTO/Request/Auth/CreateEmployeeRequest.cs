using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Auth
{
    public class CreateEmployeeRequest
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور المبدئية مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تتكون من 6 أحرف على الأقل")]
        public string TemporaryPassword { get; set; } = null!;

        // Country اختياري - إذا لم يُدخل، سيُعيّن تلقائياً
        public string? Country { get; set; }
    }
}