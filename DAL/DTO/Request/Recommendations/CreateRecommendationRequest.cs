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

        [Required(ErrorMessage = "الوصف بالعربية مطلوب")]  // ✅ أضفنا Required
        [MaxLength(1000)]
        public string? DescriptionAr { get; set; }

        [Required(ErrorMessage = "الوصف بالإنجليزية مطلوب")]  // ✅ أضفنا Required
        [MaxLength(1000)]
        public string? DescriptionEn { get; set; }

        // ✅ حذفنا [MaxLength(500)] لأنه لا يعمل مع IFormFile
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