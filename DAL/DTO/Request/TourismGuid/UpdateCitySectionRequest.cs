using System.Text.Json.Serialization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Rovaya.DAL.DTO.Request.TourismGuid
{
    public class UpdateCitySectionRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("titleAr")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("titleEn")]
        public string TitleEn { get; set; } = string.Empty;

        [JsonPropertyName("contentAr")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("contentEn")]
        public string ContentEn { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; }

        // ✅ لدعم رفع ملفات الصور
        public List<IFormFile>? ImageFiles { get; set; }
        // ✅ لدعم رفع عدة ملفات فيديو
        public List<IFormFile>? VideoFiles { get; set; }

        [JsonPropertyName("images")]
        public List<string>? ExistingImages { get; set; }

        [JsonPropertyName("videos")]
        public List<string>? ExistingVideos { get; set; }
    }
}