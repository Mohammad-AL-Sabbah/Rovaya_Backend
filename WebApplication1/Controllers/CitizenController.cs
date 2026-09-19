using Microsoft.AspNetCore.Mvc;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.DTO.Request.TourismGuid;

namespace Rovaya.PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitizenController : ControllerBase
    {
        private readonly ITourismGuidService _tourismService;

        public CitizenController(ITourismGuidService tourismService)
        {
            _tourismService = tourismService;
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
    }
}