using Microsoft.AspNetCore.Http;
using Rovaya.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Recommendations
{
    public class CreateRecommendationRequest
    {
        [Required(ErrorMessage = "العنوان بالعربية مطلوب")]
        [MaxLength(200)]
        public string TitleAr { get; set; } = null!;

        [Required(ErrorMessage = "العنوان بالإنجليزية مطلوب")]
        [MaxLength(200)]
        public string TitleEn { get; set; } = null!;

        [Required(ErrorMessage = "الوصف بالعربية مطلوب")]
        [MaxLength(1000)]
        public string? DescriptionAr { get; set; }

        [Required(ErrorMessage = "الوصف بالإنجليزية مطلوب")]
        [MaxLength(1000)]
        public string? DescriptionEn { get; set; }

        public IFormFile? ImageFile { get; set; }

        public RecommendationSeason Season { get; set; } = RecommendationSeason.All;
        public ActivityType ActivityType { get; set; } = ActivityType.Cultural;

        // ✅ المناسبة بالعربية
        [MaxLength(200)]
        public string? Occasion { get; set; }

        // ✅ المناسبة بالإنجليزية (حقل جديد)
        [MaxLength(200)]
        public string? OccasionEn { get; set; }

        public int? SuggestedPlaceId { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}