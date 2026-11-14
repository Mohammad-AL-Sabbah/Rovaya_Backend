using System.Collections.Generic;

namespace Rovaya.DAL.DTO.Response.TourismGuid
{
    public class TourismPlaceResponse
    {
        public int Id { get; set; }

        // ✅ قوائم لدعم التصنيفات المتعددة
        public List<int> CategoryIds { get; set; } = new List<int>();
        public List<string> CategoryNames { get; set; } = new List<string>();

        public int? ParentPlaceId { get; set; }
        public string? ParentPlaceName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public List<TourismPlaceResponse> SubPlaces { get; set; } = new List<TourismPlaceResponse>();
    }
}