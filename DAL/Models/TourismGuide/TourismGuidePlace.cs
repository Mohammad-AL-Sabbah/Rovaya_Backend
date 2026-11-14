using System;
using System.Collections.Generic;

namespace Rovaya.DAL.Models.TourismGuide
{
    public class TourismGuidePlace
    {
        public int Id { get; set; }

        // ✅ تم حذف: public int CategoryId { get; set; }

        public int? ParentPlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public TourismGuidePlace? ParentPlace { get; set; }
        public ICollection<TourismGuidePlace> SubPlaces { get; set; } = new List<TourismGuidePlace>();
        public ICollection<TourismGuidePlaceTranslation> Translations { get; set; } = new List<TourismGuidePlaceTranslation>();

        // ✅ إضافة علاقة Many-to-Many مع التصنيفات
        public ICollection<TourismGuideCategory> Categories { get; set; } = new List<TourismGuideCategory>();
    }
}