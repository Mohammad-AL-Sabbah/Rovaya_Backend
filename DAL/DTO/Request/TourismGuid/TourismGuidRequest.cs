using Microsoft.AspNetCore.Http;

namespace Rovaya.DAL.DTO.Request.TourismGuid
{
    public class TourismGuidRequest
    {
        public IFormFile? Image { get; set; }

        // إرسال الترجمات كـ JSON String
        // مثال: [{"languageCode":"ar","name":"نابلس","description":"مدينة نابلس"},{"languageCode":"en","name":"nablus","description":"nablus city"}]
        public string? TranslationsJson { get; set; }
    }
}