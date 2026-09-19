namespace Rovaya.DAL.DTO.Response.TourismGuid
{
    public class TourismGuidResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}