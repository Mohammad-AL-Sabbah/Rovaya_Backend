using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.DTO.Request.TourismGuid;

namespace Rovaya.PL.Controllers.Areas.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // يضمن أن فقط المستخدمين المصرح لهم يمكنهم الوصول
    public class AdminController : ControllerBase
    {
        private readonly ITourismGuidService _tourismService;

        public AdminController(ITourismGuidService tourismService)
        {
            _tourismService = tourismService;
        }

        // ================= مساعدات معالجة الأخطاء (Error Handling Helpers) =================

        private IActionResult HandleModelState()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { success = false, message = "بيانات الطلب غير صالحة", errors });
        }

        private IActionResult HandleException(Exception ex, string customMessage = "حدث خطأ غير متوقع في الخادم")
        {
            // ملاحظة: هنا يُفضل إضافة Logger لتسجيل الخطأ الحقيقي في ملفات النظام
            // _logger.LogError(ex, customMessage);

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
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في إنشاء التصنيف");
            }
        }

        [HttpPut("update-tourism-category/{id}")]
        public async Task<IActionResult> UpdateTourismCategory(int id, [FromForm] TourismGuidRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();

                var success = await _tourismService.UpdateCategoryAsync(id, request);
                if (!success)
                    return NotFound(new { success = false, message = "التصنيف المطلوب غير موجود." });

                return Ok(new { success = true, message = "تم تحديث التصنيف بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في تحديث التصنيف");
            }
        }

        [HttpDelete("delete-tourism-category/{id}")]
        public async Task<IActionResult> DeleteTourismCategory(int id)
        {
            try
            {
                var success = await _tourismService.DeleteCategoryAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = "التصنيف المطلوب غير موجود." });

                return Ok(new { success = true, message = "تم حذف التصنيف بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في حذف التصنيف");
            }
        }

        [HttpGet("get-tourism-categories")]
        public async Task<IActionResult> GetTourismCategories([FromQuery] string langCode = "ar")
        {
            try
            {
                var categories = await _tourismService.GetAllCategoriesAsync(langCode);
                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب التصنيفات");
            }
        }

        [HttpGet("get-tourism-category/{id}")]
        public async Task<IActionResult> GetTourismCategory(int id, [FromQuery] string langCode = "ar")
        {
            try
            {
                var category = await _tourismService.GetCategoryByIdAsync(id, langCode);
                if (category == null)
                    return NotFound(new { success = false, message = "التصنيف المطلوب غير موجود." });

                return Ok(new { success = true, data = category });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب تفاصيل التصنيف");
            }
        }

        // ================= Cities Management =================

        [HttpPost("create-city")]
        public async Task<IActionResult> CreateCity([FromForm] CreateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();

                var createdCity = await _tourismService.CreatePlaceAsync(request);
                return Ok(new { success = true, message = "تم إنشاء المدينة بنجاح", data = createdCity });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في إنشاء المدينة");
            }
        }

        [HttpPut("update-city/{id}")]
        public async Task<IActionResult> UpdateCity(int id, [FromForm] UpdateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();

                request.Id = id;
                var success = await _tourismService.UpdatePlaceAsync(request);
                if (!success)
                    return NotFound(new { success = false, message = "المدينة المطلوبة غير موجودة." });

                return Ok(new { success = true, message = "تم تحديث المدينة بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في تحديث المدينة");
            }
        }

        [HttpDelete("delete-city/{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            try
            {
                var success = await _tourismService.DeletePlaceAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = "المدينة المطلوبة غير موجودة." });

                return Ok(new { success = true, message = "تم حذف المدينة بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في حذف المدينة");
            }
        }

        [HttpGet("get-all-cities")]
        public async Task<IActionResult> GetAllCities(
            [FromQuery] string langCode = "ar",
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchName = null)
        {
            try
            {
                var cities = await _tourismService.GetAllCitiesAsync(langCode, categoryId, searchName);
                return Ok(new { success = true, data = cities });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب قائمة المدن");
            }
        }

        [HttpGet("get-city-details/{id}")]
        public async Task<IActionResult> GetCityDetails(int id, [FromQuery] string langCode = "ar")
        {
            try
            {
                var cityDetails = await _tourismService.GetCityDetailsAsync(id, langCode);
                if (cityDetails == null)
                    return NotFound(new { success = false, message = "تفاصيل المدينة غير موجودة." });

                return Ok(new { success = true, data = cityDetails });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب تفاصيل المدينة");
            }
        }

        // ================= Tourist Places Management =================

        [HttpPost("create-tourist-place")]
        public async Task<IActionResult> CreateTouristPlace([FromForm] CreateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();

                var createdPlace = await _tourismService.CreatePlaceAsync(request);
                return Ok(new { success = true, message = "تم إنشاء المكان السياحي بنجاح", data = createdPlace });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في إنشاء المكان السياحي");
            }
        }

        [HttpPut("update-tourist-place")]
        public async Task<IActionResult> UpdateTouristPlace([FromForm] UpdateTourismPlaceRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();

                var success = await _tourismService.UpdatePlaceAsync(request);
                if (!success)
                    return NotFound(new { success = false, message = "المكان السياحي المطلوب غير موجود." });

                return Ok(new { success = true, message = "تم تحديث المكان السياحي بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في تحديث المكان السياحي");
            }
        }

        [HttpDelete("delete-tourist-place/{id}")]
        public async Task<IActionResult> DeleteTouristPlace(int id)
        {
            try
            {
                var success = await _tourismService.DeletePlaceAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = "المكان السياحي المطلوب غير موجود." });

                return Ok(new { success = true, message = "تم حذف المكان السياحي بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في حذف المكان السياحي");
            }
        }

        [HttpGet("get-all-tourist-places")]
        public async Task<IActionResult> GetAllTouristPlaces(
            [FromQuery] string langCode = "ar",
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchName = null)
        {
            try
            {
                var searchRequest = new TourismPlaceSearchRequest
                {
                    LangCode = langCode,
                    CategoryId = categoryId,
                    SearchName = searchName
                };
                var places = await _tourismService.GetAllPlacesAsync(searchRequest);
                return Ok(new { success = true, data = places });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب الأماكن السياحية");
            }
        }

        [HttpGet("get-tourist-place/{id}")]
        public async Task<IActionResult> GetTouristPlace(int id, [FromQuery] string langCode = "ar")
        {
            try
            {
                var place = await _tourismService.GetPlaceByIdAsync(id, langCode);
                if (place == null)
                    return NotFound(new { success = false, message = "المكان السياحي المطلوب غير موجود." });

                return Ok(new { success = true, data = place });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب تفاصيل المكان السياحي");
            }
        }

        // ================= City Sections Management =================

        [HttpPost("create-city-section")]
        public async Task<IActionResult> CreateCitySection([FromBody] CreateCitySectionRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();

                var created = await _tourismService.CreateSectionAsync(request);
                return Ok(new { success = true, message = "تم إنشاء قسم المدينة بنجاح", data = created });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في إنشاء قسم المدينة");
            }
        }

        [HttpPut("update-city-section/{id}")]
        public async Task<IActionResult> UpdateCitySection(int id, [FromBody] UpdateCitySectionRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return HandleModelState();

                request.Id = id;
                var success = await _tourismService.UpdateSectionAsync(request);
                if (!success)
                    return NotFound(new { success = false, message = "قسم المدينة المطلوب غير موجود." });

                return Ok(new { success = true, message = "تم تحديث قسم المدينة بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في تحديث قسم المدينة");
            }
        }

        [HttpDelete("delete-city-section/{id}")]
        public async Task<IActionResult> DeleteCitySection(int id)
        {
            try
            {
                var success = await _tourismService.DeleteSectionAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = "قسم المدينة المطلوب غير موجود." });

                return Ok(new { success = true, message = "تم حذف قسم المدينة بنجاح." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في حذف قسم المدينة");
            }
        }

        [HttpGet("get-city-sections/{cityId}")]
        public async Task<IActionResult> GetCitySections(int cityId)
        {
            try
            {
                var sections = await _tourismService.GetSectionsByCityIdAsync(cityId);
                return Ok(new { success = true, data = sections });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب أقسام المدينة");
            }
        }

        [HttpGet("get-city-section/{id}")]
        public async Task<IActionResult> GetCitySection(int id)
        {
            try
            {
                var section = await _tourismService.GetSectionByIdAsync(id);
                if (section == null)
                    return NotFound(new { success = false, message = "قسم المدينة المطلوب غير موجود." });

                return Ok(new { success = true, data = section });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "فشل في جلب تفاصيل قسم المدينة");
            }
        }
    }
}