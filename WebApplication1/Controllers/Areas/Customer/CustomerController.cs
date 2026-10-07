using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rovaya.BLL.Service.External;
using Rovaya.BLL.Service.Recommendations;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.DTO.Request.Recommendations;
using Rovaya.DAL.DTO.Request.TourismGuid;
using Rovaya.DAL.Enums;

namespace Rovaya.PL.Controllers.Areas.Customer
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ITourismGuidService _tourismService;
        private readonly ITourismNewsService _tourismNewsService;
        private readonly IRecommendationService _recommendationService;

        public CustomerController(ITourismGuidService tourismService, ITourismNewsService tourismNewsService , IRecommendationService recommendationService)
        {
            _tourismService = tourismService;
            _tourismNewsService = tourismNewsService;
            _recommendationService = recommendationService;
        }

        // ================= Tourism Guide Categories =================

        [HttpGet("get-all-tourism-categories")]
        public async Task<IActionResult> GetAllTourismCategories([FromQuery] string langCode = "en")
        {
            var categories = await _tourismService.GetAllCategoriesAsync(langCode);
            return Ok(categories);
        }

        [HttpGet("get-tourism-category-by-id/{id}")]
        public async Task<IActionResult> GetTourismCategoryById(int id, [FromQuery] string langCode = "en")
        {
            var category = await _tourismService.GetCategoryByIdAsync(id, langCode);
            if (category == null) return NotFound("Category not found.");
            return Ok(category);
        }

        // =================== Cities (All Cities as Cards) ===================

        [HttpGet("get-all-cities")]
        public async Task<IActionResult> GetAllCities(
            [FromQuery] string langCode = "ar",
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchName = null)
        {
            var cities = await _tourismService.GetAllCitiesAsync(langCode, categoryId, searchName);
            return Ok(cities);
        }

        [HttpGet("get-city-details/{id}")]
        public async Task<IActionResult> GetCityDetails(int id, [FromQuery] string langCode = "ar")
        {
            var cityDetails = await _tourismService.GetCityDetailsAsync(id, langCode);
            if (cityDetails == null) return NotFound("City not found.");
            return Ok(cityDetails);
        }

        // ================= Tourism Guide Places & Search =================

        [HttpGet("get-all-tourism-places")]
        public async Task<IActionResult> GetAllTourismPlaces([FromQuery] TourismPlaceSearchRequest searchRequest)
        {
            var places = await _tourismService.GetAllPlacesAsync(searchRequest);
            return Ok(places);
        }

        [HttpGet("get-tourism-place-by-id/{id}")]
        public async Task<IActionResult> GetTourismPlaceById(int id, [FromQuery] string langCode = "en")
        {
            var place = await _tourismService.GetPlaceByIdAsync(id, langCode);
            if (place == null) return NotFound("Place not found.");
            return Ok(place);
        }

        // ================= Tourism News =================
        // ⚠️ IMPORTANT: Put specific routes BEFORE catch-all routes!

        /// <summary>
        /// جلب آخر أخبار السياحة من وزارة السياحة والآثار
        /// </summary>
        [HttpGet("tourism-news/latest")]  // ✅ مسار محدد
        public async Task<IActionResult> GetLatestNews([FromQuery] int count = 10)
        {
            var result = await _tourismNewsService.GetLatestNewsAsync(count);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// جلب خبر محدد بالتفصيل
        /// </summary>
        [HttpGet("tourism-news/{id:int}")]  // ✅ أضفنا :int constraint
        public async Task<IActionResult> GetNewsById(int id)
        {
            var news = await _tourismNewsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound(new { success = false, message = "الخبر غير موجود" });
            }
            return Ok(new { success = true, data = news });
        }

        /// <summary>
        /// جلب صورة الخبر
        /// </summary>
        [HttpGet("tourism-news/media/{mediaId:int}")]  // ✅ أضفنا :int constraint
        public async Task<IActionResult> GetNewsImage(int mediaId)
        {
            var imageUrl = await _tourismNewsService.GetNewsImageUrlAsync(mediaId);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return NotFound(new { success = false, message = "الصورة غير موجودة" });
            }
            return Ok(new { success = true, imageUrl });
        }





        /// <summary>
        /// جلب جميع التوصيات النشطة
        /// </summary>
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetActiveRecommendations([FromQuery] RecommendationFilterRequest filter)
        {
            var result = await _recommendationService.GetActiveRecommendationsAsync(filter);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// جلب توصية محددة
        /// </summary>
        [HttpGet("recommendations/{id}")]
        public async Task<IActionResult> GetRecommendationById(int id)
        {
            var result = await _recommendationService.GetActiveRecommendationByIdAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// جلب التوصيات حسب الموسم
        /// </summary>
        [HttpGet("recommendations/season/{season}")]
        public async Task<IActionResult> GetRecommendationsBySeason(RecommendationSeason season, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _recommendationService.GetRecommendationsBySeasonAsync(season, pageNumber, pageSize);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }



        /// <summary>
        /// جلب التوصيات حسب نوع النشاط
        /// </summary>
        /// 
        [AllowAnonymous]
        [HttpGet("recommendations/activity/{activityType}")]
        public async Task<IActionResult> GetRecommendationsByActivity(ActivityType activityType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _recommendationService.GetRecommendationsByActivityAsync(activityType, pageNumber, pageSize);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }










    }
}