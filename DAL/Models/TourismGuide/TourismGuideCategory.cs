using System;
using System.Collections.Generic;

namespace Rovaya.DAL.Models.TourismGuide
{
    public class TourismGuideCategory
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TourismGuideCategoryTranslation> TourismGuideTranslations { get; set; }
            = new List<TourismGuideCategoryTranslation>();

        public ICollection<TourismGuidePlace> Places { get; set; }
            = new List<TourismGuidePlace>();
    }
}