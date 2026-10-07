using System.Text.Json.Serialization;

namespace Rovaya.DAL.DTO.External.News
{
    public class ExternalNewsResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("date_gmt")]
        public DateTime DateGmt { get; set; }

        [JsonPropertyName("modified")]
        public DateTime Modified { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("link")]
        public string Link { get; set; } = null!;

        [JsonPropertyName("title")]
        public TitleObject Title { get; set; } = null!;

        [JsonPropertyName("content")]
        public ContentObject Content { get; set; } = null!;

        [JsonPropertyName("excerpt")]
        public ExcerptObject Excerpt { get; set; } = null!;

        [JsonPropertyName("author")]
        public int Author { get; set; }

        [JsonPropertyName("featured_media")]
        public int FeaturedMedia { get; set; }

        [JsonPropertyName("categories")]
        public List<int> Categories { get; set; } = new();

        [JsonPropertyName("_links")]
        public LinksObject? Links { get; set; }
    }

    public class TitleObject
    {
        public string Rendered { get; set; } = null!;
    }

    public class ContentObject
    {
        public string Rendered { get; set; } = null!;

        public bool Protected { get; set; }
    }

    public class ExcerptObject
    {
        public string Rendered { get; set; } = null!;

        public bool Protected { get; set; }
    }

    public class LinksObject
    {
        [JsonPropertyName("wp:featuredmedia")]
        public List<FeaturedMediaLink>? WpFeaturedMedia { get; set; }

        [JsonPropertyName("self")]
        public List<SelfLink>? Self { get; set; }
    }

    public class FeaturedMediaLink
    {
        [JsonPropertyName("embeddable")]
        public bool Embeddable { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; } = null!;
    }

    public class SelfLink
    {
        [JsonPropertyName("href")]
        public string Href { get; set; } = null!;
    }
}