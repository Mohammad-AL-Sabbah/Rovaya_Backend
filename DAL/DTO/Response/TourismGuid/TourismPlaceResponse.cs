namespace Rovaya.DAL.DTO.Response.TourismGuid
{
    public class TourismPlaceResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? ParentPlaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public List<TourismPlaceResponse> SubPlaces { get; set; } = new List<TourismPlaceResponse>();
    }
}