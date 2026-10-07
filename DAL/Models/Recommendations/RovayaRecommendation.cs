using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.TourismGuide;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rovaya.DAL.Models.Recommendations
{
    public class RovayaRecommendation
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string TitleAr { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string TitleEn { get; set; } = null!;

        [MaxLength(1000)]
        public string? DescriptionAr { get; set; }

        [MaxLength(1000)]
        public string? DescriptionEn { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public RecommendationSeason Season { get; set; } = RecommendationSeason.All;
        public ActivityType ActivityType { get; set; } = ActivityType.Cultural;

        [MaxLength(200)]
        public string? Occasion { get; set; } // مثال: "رمضان"، "عيد الفطر"

        // ربط اختياري بمكان سياحي موجود
        [ForeignKey(nameof(SuggestedPlace))]
        public int? SuggestedPlaceId { get; set; }
        public TourismGuidePlace? SuggestedPlace { get; set; }

        // ترتيب العرض (الأصغر = الأول)
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CreatedBy))]
        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}