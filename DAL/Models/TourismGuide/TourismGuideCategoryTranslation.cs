using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rovaya.DAL.Models.TourismGuide
{
    public class TourismGuideCategoryTranslation
    {
        public int Id { get; set; }
        public int TourismGuideCategoryId { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TourismGuideCategory TourismGuideCategory { get; set; } = null!;
    }
}
