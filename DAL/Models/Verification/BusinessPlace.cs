using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rovaya.DAL.Models.Business
{
    /// <summary>
    /// مكان تجاري يضيفه مستخدم موثّق مباشرة على الخريطة
    /// </summary>
    public class BusinessPlace
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Owner))]
        public string OwnerId { get; set; } = null!;
        public ApplicationUser Owner { get; set; } = null!;

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

        /// <summary>
        /// روابط الصور مخزنة كـ JSON Array
        /// </summary>
        [MaxLength(4000)]
        public string? ImagesJson { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}