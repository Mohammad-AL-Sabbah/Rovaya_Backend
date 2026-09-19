using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Rovaya.DAL.DTO.Response.TourismGuid
{
    public class CityDetailsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }

        // الأقسام الديناميكية
        public List<CitySectionResponse> Sections { get; set; } = new();

        // الأماكن السياحية داخل المدينة
        public List<TourismPlaceResponse> Places { get; set; } = new();
    }
}