using Rovaya.DAL.DTO.Response;

namespace Rovaya.DAL.DTO.Response.Recommendations
{
    public class RecommendationListResponse : BaseResponse
    {
        public List<RecommendationSummaryResponse>? Recommendations { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class RecommendationSummaryResponse
    {
        public int Id { get; set; }
        public string TitleAr { get; set; } = null!;
        public string TitleEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? ImageUrl { get; set; }
        public string Season { get; set; } = null!;
        public string ActivityType { get; set; } = null!;
        public string? Occasion { get; set; }
        public int? SuggestedPlaceId { get; set; }
        public string? SuggestedPlaceName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}