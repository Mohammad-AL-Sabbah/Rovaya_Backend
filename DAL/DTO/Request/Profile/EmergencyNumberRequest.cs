using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Profile
{
    public class EmergencyNumberRequest
    {
        [Required(ErrorMessage = "رقم الطوارئ مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = null!;
    }
}