using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rovaya.DAL.DTO.Request.TourismGuid
{
    public class TourismPlaceSearchRequest
    {
        public string LangCode { get; set; } = "ar";
        public int? CategoryId { get; set; }
        public string? SearchName { get; set; }
    }
}