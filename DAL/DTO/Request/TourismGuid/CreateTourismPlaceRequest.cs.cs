using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.TourismGuid
{
    public class CreateTourismPlaceRequest
    {
        // ✅ تم التغيير من int إلى List<int> للسماح بعدة تصنيفات أو لا شيء
        public List<int>? CategoryIds { get; set; }

        public int? ParentPlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IFormFile? Image { get; set; }

        // إرسال الترجمات كـ JSON String
        public string? TranslationsJson { get; set; }
    }
}