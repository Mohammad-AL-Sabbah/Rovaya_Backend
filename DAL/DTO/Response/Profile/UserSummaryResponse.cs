using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.Enums;

namespace Rovaya.DAL.DTO.Response.Profile
{
    public class UserSummaryResponse
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Country { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public AccountStatus Status { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}