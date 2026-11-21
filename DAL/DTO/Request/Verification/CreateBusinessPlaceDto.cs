using Microsoft.AspNetCore.Http;
using Rovaya.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Business
{
    public class CreateBusinessPlaceDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        public BusinessCategory Category { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? WorkingHours { get; set; }

        public List<IFormFile>? Images { get; set; }
    }

    public class UpdateBusinessPlaceDto
    {
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;
        [Required]
        public BusinessCategory Category { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [MaxLength(300)]
        public string? Address { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        [MaxLength(50)]
        public string? Phone { get; set; }
        [MaxLength(200)]
        public string? WorkingHours { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string>? ExistingImages { get; set; }
        public List<IFormFile>? NewImages { get; set; }
    }
}