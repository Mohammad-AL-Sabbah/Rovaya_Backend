using Rovaya.DAL.Enums;

namespace Rovaya.DAL.DTO.Response.Business
{
    public class BusinessPlaceResponseDto
    {
        public int Id { get; set; }
        public string OwnerId { get; set; } = null!;
        public string? OwnerName { get; set; }
        public string Name { get; set; } = null!;
        public BusinessCategory Category { get; set; }
        public string CategoryLabel { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public string? WorkingHours { get; set; }
        public List<string> Images { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}