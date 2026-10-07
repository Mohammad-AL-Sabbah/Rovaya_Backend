using Rovaya.DAL.Enums;

namespace Rovaya.DAL.DTO.Request.Recommendations
{
    public class RecommendationFilterRequest
    {
        public RecommendationSeason? Season { get; set; }
        public ActivityType? ActivityType { get; set; }
        public string? Occasion { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}