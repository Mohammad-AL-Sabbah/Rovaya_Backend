using Microsoft.AspNetCore.Http;

namespace Rovaya.DAL.DTO.Request.TourismGuid
{
    public class CreateTourismPlaceRequest
    {
        public int CategoryId { get; set; }
        public int? ParentPlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IFormFile? Image { get; set; }

        // إرسال الترجمات كـ JSON String
        public string? TranslationsJson { get; set; }
    }
}