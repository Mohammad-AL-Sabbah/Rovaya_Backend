using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rovaya.BLL.Service.Recommendations;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.DTO.Request.Auth;
using Rovaya.DAL.DTO.Request.Recommendations;
using Rovaya.DAL.DTO.Request.TourismGuid;
using Rovaya.DAL.DTO.Response.Profile;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Repository.Profile;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rovaya.PL.Controllers.Areas.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly ITourismGuidService _tourismService;
        private readonly IRecommendationService _recommendationService;
        private readonly IProfileRepository _profileRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ITourismGuidService tourismService,
            IRecommendationService recommendationService,
            IProfileRepository profileRepository,
            UserManager<ApplicationUser> userManager)
        {
            _tourismService = tourismService;
            _recommendationService = recommendationService;
            _profileRepository = profileRepository;
            _userManager = userManager;
        }

        private IActionResult HandleModelState()
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = "بيانات الطلب غير صالحة", errors });
        }

        private IActionResult HandleException(Exception ex, string customMessage = "حدث خطأ غير متوقع في الخادم")
        {
            return StatusCode(500, new { success = false, message = customMessage, details = ex.Message });
        }

        // ================= Tourism Guide Categories =================

        [HttpPost("create-tourism-category")]
        public async Task<IActionResult> CreateTourismCategory([FromForm] TourismGuidRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var createdCategory = await _tourismService.CreateCategoryAsync(request);
                return Ok(new { success = true, message = "تم إنشاء التصنيف بنجاح", data = createdCategory });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في إنشاء التصنيف"); }
        }

        [HttpPut("update-tourism-category/{id}")]
        public async Task<IActionResult> UpdateTourismCategory(int id, [FromForm] TourismGuidRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var success = await _tourismService.UpdateCategoryAsync(id, request);
                if (!success) return NotFound(new { success = false, message = "التصنيف المطلوب غير موجود." });
                return Ok(new { success = true, message = "تم تحديث التصنيف بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في تحديث التصنيف"); }
        }

        [HttpDelete("delete-tourism-category/{id}")]
        public async Task<IActionResult> DeleteTourismCategory(int id)
        {
            try
            {
                var success = await _tourismService.DeleteCategoryAsync(id);
                if (!success) return NotFound(new { success = false, message = "التصنيف المطلوب غير موجود." });
                return Ok(new { success = true, message = "تم حذف التصنيف بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في حذف التصنيف"); }
        }

        [HttpGet("get-tourism-categories")]
        public async Task<IActionResult> GetTourismCategories([FromQuery] string langCode = "ar")
        {
            try
            {
                var categories = await _tourismService.GetAllCategoriesAsync(langCode);
                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب التصنيفات"); }
        }

        [HttpGet("get-tourism-category/{id}")]
        public async Task<IActionResult> GetTourismCategory(int id, [FromQuery] string langCode = "ar")
        {
            try
            {
                var category = await _tourismService.GetCategoryByIdAsync(id, langCode);
                if (category == null) return NotFound(new { success = false, message = "التصنيف المطلوب غير موجود." });
                return Ok(new { success = true, data = category });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب تفاصيل التصنيف"); }
        }

        // ================= Cities & Places Management =================

        [HttpPost("create-city")]
        public async Task<IActionResult> CreateCity([FromForm] CreateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var createdCity = await _tourismService.CreatePlaceAsync(request);
                return Ok(new { success = true, message = "تم إنشاء المدينة بنجاح", data = createdCity });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في إنشاء المدينة"); }
        }

        [HttpPut("update-city/{id}")]
        public async Task<IActionResult> UpdateCity(int id, [FromForm] UpdateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                request.Id = id;
                var success = await _tourismService.UpdatePlaceAsync(request);
                if (!success) return NotFound(new { success = false, message = "المدينة المطلوبة غير موجودة." });
                return Ok(new { success = true, message = "تم تحديث المدينة بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في تحديث المدينة"); }
        }

        [HttpDelete("delete-city/{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            try
            {
                var success = await _tourismService.DeletePlaceAsync(id);
                if (!success) return NotFound(new { success = false, message = "المدينة المطلوبة غير موجودة." });
                return Ok(new { success = true, message = "تم حذف المدينة بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في حذف المدينة"); }
        }

        [HttpGet("get-all-cities")]
        public async Task<IActionResult> GetAllCities([FromQuery] string langCode = "ar", [FromQuery] int? categoryId = null, [FromQuery] string? searchName = null)
        {
            try
            {
                var cities = await _tourismService.GetAllCitiesAsync(langCode, categoryId, searchName);
                return Ok(new { success = true, data = cities });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب قائمة المدن"); }
        }

        [HttpGet("get-city-details/{id}")]
        public async Task<IActionResult> GetCityDetails(int id, [FromQuery] string langCode = "ar")
        {
            try
            {
                var cityDetails = await _tourismService.GetCityDetailsAsync(id, langCode);
                if (cityDetails == null) return NotFound(new { success = false, message = "تفاصيل المدينة غير موجودة." });
                return Ok(new { success = true, data = cityDetails });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب تفاصيل المدينة"); }
        }

        [HttpPost("create-tourist-place")]
        public async Task<IActionResult> CreateTouristPlace([FromForm] CreateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var createdPlace = await _tourismService.CreatePlaceAsync(request);
                return Ok(new { success = true, message = "تم إنشاء المكان السياحي بنجاح", data = createdPlace });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في إنشاء المكان السياحي"); }
        }

        [HttpPut("update-tourist-place")]
        public async Task<IActionResult> UpdateTouristPlace([FromForm] UpdateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var success = await _tourismService.UpdatePlaceAsync(request);
                if (!success) return NotFound(new { success = false, message = "المكان السياحي المطلوب غير موجود." });
                return Ok(new { success = true, message = "تم تحديث المكان السياحي بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في تحديث المكان السياحي"); }
        }

        [HttpDelete("delete-tourist-place/{id}")]
        public async Task<IActionResult> DeleteTouristPlace(int id)
        {
            try
            {
                var success = await _tourismService.DeletePlaceAsync(id);
                if (!success) return NotFound(new { success = false, message = "المكان السياحي المطلوب غير موجود." });
                return Ok(new { success = true, message = "تم حذف المكان السياحي بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في حذف المكان السياحي"); }
        }

        [HttpGet("get-all-tourist-places")]
        public async Task<IActionResult> GetAllTouristPlaces([FromQuery] string langCode = "ar", [FromQuery] int? categoryId = null, [FromQuery] string? searchName = null)
        {
            try
            {
                var searchRequest = new TourismPlaceSearchRequest { LangCode = langCode, CategoryId = categoryId, SearchName = searchName };
                var places = await _tourismService.GetAllPlacesAsync(searchRequest);
                return Ok(new { success = true, data = places });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب الأماكن السياحية"); }
        }

        [HttpGet("get-tourist-place/{id}")]
        public async Task<IActionResult> GetTouristPlace(int id, [FromQuery] string langCode = "ar")
        {
            try
            {
                var place = await _tourismService.GetPlaceByIdAsync(id, langCode);
                if (place == null) return NotFound(new { success = false, message = "المكان السياحي المطلوب غير موجود." });
                return Ok(new { success = true, data = place });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب تفاصيل المكان السياحي"); }
        }

        // ================= City Sections Management =================

        [HttpPost("create-city-section")]
        public async Task<IActionResult> CreateCitySection([FromForm] CreateCitySectionRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var created = await _tourismService.CreateSectionAsync(request);
                return Ok(new { success = true, message = "تم إنشاء قسم المدينة بنجاح", data = created });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في إنشاء قسم المدينة"); }
        }


        [HttpPut("update-city-section/{id}")]
        public async Task<IActionResult> UpdateCitySection(int id, [FromForm] UpdateCitySectionRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                request.Id = id;
                var success = await _tourismService.UpdateSectionAsync(request);
                if (!success) return NotFound(new { success = false, message = "قسم المدينة المطلوب غير موجود." });
                return Ok(new { success = true, message = "تم تحديث قسم المدينة بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في تحديث قسم المدينة"); }
        }

        [HttpDelete("delete-city-section/{id}")]
        public async Task<IActionResult> DeleteCitySection(int id)
        {
            try
            {
                var success = await _tourismService.DeleteSectionAsync(id);
                if (!success) return NotFound(new { success = false, message = "قسم المدينة المطلوب غير موجود." });
                return Ok(new { success = true, message = "تم حذف قسم المدينة بنجاح." });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في حذف قسم المدينة"); }
        }

        [HttpGet("get-city-sections/{cityId}")]
        public async Task<IActionResult> GetCitySections(int cityId)
        {
            try
            {
                var sections = await _tourismService.GetSectionsByCityIdAsync(cityId);
                return Ok(new { success = true, data = sections });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب أقسام المدينة"); }
        }

        [HttpGet("get-city-section/{id}")]
        public async Task<IActionResult> GetCitySection(int id)
        {
            try
            {
                var section = await _tourismService.GetSectionByIdAsync(id);
                if (section == null) return NotFound(new { success = false, message = "قسم المدينة المطلوب غير موجود." });
                return Ok(new { success = true, data = section });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب تفاصيل قسم المدينة"); }
        }

        // ================= Recommendations Management =================

        [HttpPost("create-recommendation")]
        public async Task<IActionResult> CreateRecommendation([FromForm] CreateRecommendationRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var result = await _recommendationService.CreateRecommendationAsync(User, request);
                if (!result.Success) return BadRequest(result);
                return Ok(new { success = true, message = result.Message, data = result });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في إنشاء التوصية"); }
        }

        [HttpPut("update-recommendation")]
        public async Task<IActionResult> UpdateRecommendation([FromForm] UpdateRecommendationRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();
                var result = await _recommendationService.UpdateRecommendationAsync(User, request);
                if (!result.Success) return BadRequest(result);
                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في تحديث التوصية"); }
        }

        [HttpDelete("delete-recommendation/{id}")]
        public async Task<IActionResult> DeleteRecommendation(int id)
        {
            try
            {
                var result = await _recommendationService.DeleteRecommendationAsync(User, id);
                if (!result.Success) return BadRequest(result);
                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في حذف التوصية"); }
        }

        [HttpGet("get-all-recommendations")]
        public async Task<IActionResult> GetAllRecommendations([FromQuery] RecommendationFilterRequest filter)
        {
            try
            {
                var result = await _recommendationService.GetAllRecommendationsAsync(User, filter);
                if (!result.Success) return BadRequest(result);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب التوصيات"); }
        }

        [HttpGet("get-recommendation/{id}")]
        public async Task<IActionResult> GetRecommendationById(int id)
        {
            try
            {
                var result = await _recommendationService.GetRecommendationByIdAsync(User, id);
                if (!result.Success) return BadRequest(result);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex) { return HandleException(ex, "فشل في جلب تفاصيل التوصية"); }
        }

        // ================= Employees Management =================

        [HttpGet("get-all-employees")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var allUsers = await _profileRepository.GetAllUsersAsync();
                var employees = new List<UserSummaryResponse>();

                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                    {
                        employees.Add(new UserSummaryResponse
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            Country = user.Country,
                            CreatedAt = user.CreatedAt,
                            Status = user.Status,
                            Roles = roles.ToList()
                        });
                    }
                }

                return Ok(new { success = true, message = $"تم جلب {employees.Count} موظف وإداري بنجاح", data = employees, totalCount = employees.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"حدث خطأ في الخادم: {ex.Message}" });
            }
        }
    }
}