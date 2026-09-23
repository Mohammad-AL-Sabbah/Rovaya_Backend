using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأول لا يمكن أن يتجاوز 50 حرفاً")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        [StringLength(50, ErrorMessage = "اسم العائلة لا يمكن أن يتجاوز 50 حرفاً")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تتكون من 6 أحرف على الأقل")]
        // يمكنك إضافة RegularExpression هنا لفرض تعقيد أكبر (أحرف كبيرة، أرقام، رموز) إذا رغبت
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [StringLength(20, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز 20 رقماً")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "الدولة مطلوبة")]
        [StringLength(100, ErrorMessage = "اسم الدولة لا يمكن أن يتجاوز 100 حرف")]
        public string Country { get; set; } = null!;


    }
}