namespace Rovaya.DAL.Models.TourismGuide
{
    public class TourismGuidePlace
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? ParentPlaceId { get; set; }

        // الخصائص الناقصة التي تسببت بالخطأ الأول
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public TourismGuideCategory Category { get; set; } = null!;
        public TourismGuidePlace? ParentPlace { get; set; }
        public ICollection<TourismGuidePlace> SubPlaces { get; set; } = new List<TourismGuidePlace>();
        public ICollection<TourismGuidePlaceTranslation> Translations { get; set; } = new List<TourismGuidePlaceTranslation>();
    }
}