using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rovaya.DAL.Models.TourismGuide
{
    public class CitySection
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ImagesJson { get; set; }
        public string? VideosJson { get; set; }

        [ForeignKey(nameof(CityId))]
        public TourismGuidePlace City { get; set; } = null!;
    }
}