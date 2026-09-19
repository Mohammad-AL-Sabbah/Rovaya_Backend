using Microsoft.AspNetCore.Mvc;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.DTO.Request.TourismGuid;

namespace Rovaya.PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ITourismGuidService _tourismService;

        public AdminController(ITourismGuidService tourismService)
        {
            _tourismService = tourismService;
        }

        // ================= Tourism Guide Categories =================

        [HttpPost("create-tourism-category")]
        public async Task<IActionResult> CreateTourismCategory([FromForm] TourismGuidRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdCategory = await _tourismService.CreateCategoryAsync(request);
            return Ok(createdCategory);
        }

        [HttpPut("update-tourism-category/{id}")]
        public async Task<IActionResult> UpdateTourismCategory(int id, [FromForm] TourismGuidRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _tourismService.UpdateCategoryAsync(id, request);
            if (!success) return NotFound("Category not found.");
            return Ok("Category updated successfully.");
        }

        [HttpDelete("delete-tourism-category/{id}")]
        public async Task<IActionResult> DeleteTourismCategory(int id)
        {
            var success = await _tourismService.DeleteCategoryAsync(id);
            if (!success) return NotFound("Category not found.");
            return Ok("Category deleted successfully.");
        }

        [HttpGet("get-tourism-categories")]
        public async Task<IActionResult> GetTourismCategories([FromQuery] string langCode = "ar")
        {
            var categories = await _tourismService.GetAllCategoriesAsync(langCode);
            return Ok(categories);
        }

        [HttpGet("get-tourism-category/{id}")]
        public async Task<IActionResult> GetTourismCategory(int id, [FromQuery] string langCode = "ar")
        {
            var category = await _tourismService.GetCategoryByIdAsync(id, langCode);
            if (category == null) return NotFound("Category not found.");
            return Ok(category);
        }

        // ================= Cities Management =================

        [HttpPost("create-city")]
        public async Task<IActionResult> CreateCity([FromForm] CreateTourismPlaceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdCity = await _tourismService.CreatePlaceAsync(request);
            return Ok(createdCity);
        }

        [HttpPut("update-city/{id}")]
        public async Task<IActionResult> UpdateCity(int id, [FromForm] UpdateTourismPlaceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            request.Id = id;
            var success = await _tourismService.UpdatePlaceAsync(request);
            if (!success) return NotFound("City not found.");
            return Ok("City updated successfully.");
        }

        [HttpDelete("delete-city/{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var success = await _tourismService.DeletePlaceAsync(id);
            if (!success) return NotFound("City not found.");
            return Ok("City deleted successfully.");
        }

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

        // ================= Tourist Places Management =================

        [HttpPost("create-tourist-place")]
        public async Task<IActionResult> CreateTouristPlace([FromForm] CreateTourismPlaceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdPlace = await _tourismService.CreatePlaceAsync(request);
            return Ok(createdPlace);
        }

        [HttpPut("update-tourist-place")]
        public async Task<IActionResult> UpdateTouristPlace([FromForm] UpdateTourismPlaceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _tourismService.UpdatePlaceAsync(request);
            if (!success) return NotFound("Place not found.");
            return Ok("Place updated successfully.");
        }

        [HttpDelete("delete-tourist-place/{id}")]
        public async Task<IActionResult> DeleteTouristPlace(int id)
        {
            var success = await _tourismService.DeletePlaceAsync(id);
            if (!success) return NotFound("Place not found.");
            return Ok("Place deleted successfully.");
        }

        [HttpGet("get-all-tourist-places")]
        public async Task<IActionResult> GetAllTouristPlaces(
            [FromQuery] string langCode = "ar",
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchName = null)
        {
            var searchRequest = new TourismPlaceSearchRequest
            {
                LangCode = langCode,
                CategoryId = categoryId,
                SearchName = searchName
            };
            var places = await _tourismService.GetAllPlacesAsync(searchRequest);
            return Ok(places);
        }

        [HttpGet("get-tourist-place/{id}")]
        public async Task<IActionResult> GetTouristPlace(int id, [FromQuery] string langCode = "ar")
        {
            var place = await _tourismService.GetPlaceByIdAsync(id, langCode);
            if (place == null) return NotFound("Place not found.");
            return Ok(place);
        }

        // ================= City Sections Management =================

        [HttpPost("create-city-section")]
        public async Task<IActionResult> CreateCitySection([FromBody] CreateCitySectionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _tourismService.CreateSectionAsync(request);
            return Ok(created);
        }

        [HttpPut("update-city-section/{id}")]
        public async Task<IActionResult> UpdateCitySection(int id, [FromBody] UpdateCitySectionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            request.Id = id;
            var success = await _tourismService.UpdateSectionAsync(request);
            if (!success) return NotFound("Section not found.");
            return Ok("Section updated successfully.");
        }

        [HttpDelete("delete-city-section/{id}")]
        public async Task<IActionResult> DeleteCitySection(int id)
        {
            var success = await _tourismService.DeleteSectionAsync(id);
            if (!success) return NotFound("Section not found.");
            return Ok("Section deleted successfully.");
        }

        [HttpGet("get-city-sections/{cityId}")]
        public async Task<IActionResult> GetCitySections(int cityId)
        {
            var sections = await _tourismService.GetSectionsByCityIdAsync(cityId);
            return Ok(sections);
        }

        [HttpGet("get-city-section/{id}")]
        public async Task<IActionResult> GetCitySection(int id)
        {
            var section = await _tourismService.GetSectionByIdAsync(id);
            if (section == null) return NotFound("Section not found.");
            return Ok(section);
        }
    }
}