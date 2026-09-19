using System.Text.Json.Serialization;

namespace Rovaya.DAL.DTO.Request.TourismGuid
{
    public class UpdateCitySectionRequest
    {
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }

        [JsonPropertyName("videos")]
        public List<string>? Videos { get; set; }
    }
}