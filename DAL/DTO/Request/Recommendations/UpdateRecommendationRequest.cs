using Microsoft.AspNetCore.Http;
using Rovaya.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Recommendations
{
    public class UpdateRecommendationRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string TitleAr { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string TitleEn { get; set; } = null!;

        [Required]  // ✅ أضفنا Required
        [MaxLength(1000)]
        public string? DescriptionAr { get; set; }

        [Required]  // ✅ أضفنا Required
        [MaxLength(1000)]
        public string? DescriptionEn { get; set; }

        // ✅ حذفنا [MaxLength(500)]
        public IFormFile? ImageFile { get; set; }

        public RecommendationSeason Season { get; set; } = RecommendationSeason.All;
        public ActivityType ActivityType { get; set; } = ActivityType.Cultural;

        [MaxLength(200)]
        public string? Occasion { get; set; }

        public int? SuggestedPlaceId { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}