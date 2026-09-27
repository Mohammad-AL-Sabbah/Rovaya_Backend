using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Auth
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        [Display(Name = "كلمة المرور الحالية")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور الجديدة يجب أن تتكون من 6 أحرف على الأقل")]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "تأكيد كلمة المرور الجديدة مطلوب")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيدها غير متطابقين")]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}