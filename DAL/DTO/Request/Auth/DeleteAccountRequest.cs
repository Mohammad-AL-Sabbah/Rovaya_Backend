using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Auth
{
    public class DeleteAccountRequest
    {
        [Required(ErrorMessage = "كلمة المرور مطلوبة لتأكيد حذف الحساب")]
        public string Password { get; set; } = null!;
    }
}