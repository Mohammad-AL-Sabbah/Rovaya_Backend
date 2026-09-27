using System.ComponentModel.DataAnnotations;
using Rovaya.DAL.Enums;

namespace Rovaya.DAL.DTO.Request.Auth
{
    public class BlockUserRequest
    {
        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        public string TargetUserId { get; set; } = null!;

        [Range(1, 1000, ErrorMessage = "يجب أن تكون المدة بين 1 و 1000")]
        public int? DurationValue { get; set; }

        // الآن سيقبل هذا الحقل نصوصاً مثل "Days", "Hours", إلخ.
        public BlockDurationUnit? DurationUnit { get; set; }

        [Required(ErrorMessage = "سبب الحظر مطلوب")]
        [MaxLength(500, ErrorMessage = "سبب الحظر لا يمكن أن يتجاوز 500 حرف")]
        public string Reason { get; set; } = null!;
    }
}