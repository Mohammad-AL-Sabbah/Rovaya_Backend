using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.Enums;

namespace Rovaya.DAL.DTO.Response.Recommendations
{
    public class RecommendationResponse : BaseResponse
    {
        public int Id { get; set; }
        public string TitleAr { get; set; } = null!;
        public string TitleEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? ImageUrl { get; set; }
        public RecommendationSeason Season { get; set; }
        public ActivityType ActivityType { get; set; }
        public string? Occasion { get; set; }
        public int? SuggestedPlaceId { get; set; }
        public string? SuggestedPlaceName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}