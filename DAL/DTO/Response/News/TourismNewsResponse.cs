using Rovaya.DAL.DTO.Response;

namespace Rovaya.DAL.DTO.Response.News
{
    public class TourismNewsResponse : BaseResponse
    {
        public List<TourismNewsItem>? Articles { get; set; }
        public int TotalCount { get; set; }
    }

    public class TourismNewsItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Slug { get; set; } = null!;
        public string SourceLink { get; set; } = null!;
        public List<int> Categories { get; set; } = new();
    }
}