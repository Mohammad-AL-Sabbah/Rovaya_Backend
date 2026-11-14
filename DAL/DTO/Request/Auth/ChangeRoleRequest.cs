using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Auth
{
    public class ChangeRoleRequest
    {
        [Required]
        public string TargetUserId { get; set; } = null!;

        [Required]
        public string NewRole { get; set; } = null!; // "Admin" أو "SuperAdmin"
    }
}