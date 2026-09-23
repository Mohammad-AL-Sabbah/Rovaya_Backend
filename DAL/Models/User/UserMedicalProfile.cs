using Rovaya.DAL.Models.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rovaya.DAL.Models.Identity
{
    public class UserMedicalProfile
    {
        public int Id { get; set; }

        // Foreign Key
        [ForeignKey(nameof(ApplicationUser))]
        public string ApplicationUserId { get; set; } = null!;

        // Navigation Property
        public ApplicationUser ApplicationUser { get; set; } = null!;

        // الأمراض المزمنة
        [MaxLength(500)]
        public string? ChronicDiseases { get; set; }

        // الأدوية الحالية
        [MaxLength(500)]
        public string? CurrentMedications { get; set; }

        // الحساسية
        [MaxLength(500)]
        public string? Allergies { get; set; }

        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;
    }
}