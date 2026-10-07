using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.Models.Identity;

namespace Rovaya.DAL.DTO.Response.Profile
{
    public class ProfileResponse : BaseResponse
    {
        public string? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? AdditionalPhoneNumbers { get; set; }
        public List<string>? EmergencyNumbers { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public MedicalProfileResponse? MedicalProfile { get; set; }
    }
}