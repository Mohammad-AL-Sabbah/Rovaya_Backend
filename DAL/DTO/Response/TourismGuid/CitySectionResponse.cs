using System;
using System.Collections.Generic;

namespace Rovaya.DAL.DTO.Response.TourismGuid
{
    public class CitySectionResponse
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ContentEn { get; set; } = string.Empty; 
        public int Order { get; set; }
        public List<string> Images { get; set; } = new();
        public List<string> Videos { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}