using Rovaya.DAL.Enums;
using System.Text.Json;

namespace Rovaya.DAL.DTO.Response
{
    public class BusinessRequestResponseDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public VerificationStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Images { get; set; } = new();
    }
}