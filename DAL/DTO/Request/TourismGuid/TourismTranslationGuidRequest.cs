using System.Text.Json.Serialization;

namespace Rovaya.DAL.DTO.Request.TourismGuid
{
    public class TourismTranslationGuidRequest
    {
        [JsonPropertyName("languageCode")]
        public string LanguageCode { get; set; } = "ar";

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}