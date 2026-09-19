namespace Rovaya.DAL.Models.TourismGuide
{
    public class TourismGuidePlaceTranslation
    {
        public int Id { get; set; }
        public int TourismGuidePlaceId { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public TourismGuidePlace TourismGuidePlace { get; set; } = null!;
    }
}