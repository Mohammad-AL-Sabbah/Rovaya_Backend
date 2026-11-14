using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Rovaya.DAL.DTO.Request.Recommendations;
using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.DTO.Response.Recommendations;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Recommendations;
using Rovaya.DAL.Repository.Recommendations;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Recommendations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IRecommendationRepository _recommendationRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public RecommendationService(
            IRecommendationRepository recommendationRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _recommendationRepository = recommendationRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;
            return await _userManager.FindByIdAsync(userId);
        }

        private async Task<bool> IsAdminOrSuperAdminAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return false;

            var roles = await _userManager.GetRolesAsync(currentUser);
            return roles.Contains("Admin") || roles.Contains("SuperAdmin");
        }

        private async Task<string?> HandleImageUploadAsync(IFormFile? file, string? oldFileName = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return oldFileName;
                }

                if (!string.IsNullOrEmpty(oldFileName))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "recommendations", oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "recommendations");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return uniqueFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error uploading file: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        private string GetFullImageUrl(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return string.Empty;
            if (fileName.StartsWith("http://") || fileName.StartsWith("https://"))
            {
                return fileName;
            }

            var appUrl = _configuration["AppUrl"];
            if (!appUrl.StartsWith("http://") && !appUrl.StartsWith("https://"))
            {
                appUrl = "https://" + appUrl;
            }

            return $"{appUrl}/uploads/recommendations/{fileName}";
        }

        private RecommendationResponse MapToFullResponse(RovayaRecommendation rec)
        {
            return new RecommendationResponse
            {
                Success = true,
                Message = "تم جلب التوصية بنجاح",
                Id = rec.Id,
                TitleAr = rec.TitleAr,
                TitleEn = rec.TitleEn,
                DescriptionAr = rec.DescriptionAr,
                DescriptionEn = rec.DescriptionEn,
                ImageUrl = GetFullImageUrl(rec.ImageUrl),
                Season = rec.Season,
                ActivityType = rec.ActivityType,
                Occasion = rec.Occasion,
                OccasionEn = rec.OccasionEn, // ✅ إضافة المناسبة الإنجليزية
                SuggestedPlaceId = rec.SuggestedPlaceId,
                SuggestedPlaceName = rec.SuggestedPlace?.Translations?.FirstOrDefault(t => t.LanguageCode == "ar")?.Name,
                DisplayOrder = rec.DisplayOrder,
                IsActive = rec.IsActive,
                CreatedByUserName = rec.CreatedBy != null ? $"{rec.CreatedBy.FirstName} {rec.CreatedBy.LastName}" : null,
                CreatedAt = rec.CreatedAt,
                UpdatedAt = rec.UpdatedAt
            };
        }

        private RecommendationSummaryResponse MapToSummaryResponse(RovayaRecommendation rec)
        {
            return new RecommendationSummaryResponse
            {
                Id = rec.Id,
                TitleAr = rec.TitleAr,
                TitleEn = rec.TitleEn,
                DescriptionAr = rec.DescriptionAr,
                DescriptionEn = rec.DescriptionEn,
                ImageUrl = GetFullImageUrl(rec.ImageUrl),
                Season = rec.Season.ToString(),
                ActivityType = rec.ActivityType.ToString(),
                Occasion = rec.Occasion,
                OccasionEn = rec.OccasionEn, // ✅ إضافة المناسبة الإنجليزية
                SuggestedPlaceId = rec.SuggestedPlaceId,
                SuggestedPlaceName = rec.SuggestedPlace?.Translations?.FirstOrDefault(t => t.LanguageCode == "ar")?.Name,
                DisplayOrder = rec.DisplayOrder,
                IsActive = rec.IsActive,
                CreatedAt = rec.CreatedAt
            };
        }

        // ==================== Admin Operations ====================

        public async Task<RecommendationResponse> CreateRecommendationAsync(ClaimsPrincipal adminUser, CreateRecommendationRequest request)
        {
            try
            {
                if (!await IsAdminOrSuperAdminAsync(adminUser))
                {
                    return new RecommendationResponse { Success = false, Message = "لا تملك صلاحية إنشاء توصيات" };
                }

                var currentUser = await GetCurrentUserAsync(adminUser);
                var fileName = await HandleImageUploadAsync(request.ImageFile);

                var recommendation = new RovayaRecommendation
                {
                    TitleAr = request.TitleAr,
                    TitleEn = request.TitleEn,
                    DescriptionAr = request.DescriptionAr,
                    DescriptionEn = request.DescriptionEn,
                    ImageUrl = fileName,
                    Season = request.Season,
                    ActivityType = request.ActivityType,
                    Occasion = request.Occasion,
                    OccasionEn = request.OccasionEn, // ✅ حفظ المناسبة الإنجليزية
                    SuggestedPlaceId = request.SuggestedPlaceId,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive,
                    CreatedByUserId = currentUser?.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _recommendationRepository.CreateAsync(recommendation);
                var response = MapToFullResponse(created);
                response.Message = "تم إنشاء التوصية بنجاح";

                return response;
            }
            catch (Exception ex)
            {
                return new RecommendationResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        public async Task<RecommendationResponse> UpdateRecommendationAsync(ClaimsPrincipal adminUser, UpdateRecommendationRequest request)
        {
            try
            {
                if (!await IsAdminOrSuperAdminAsync(adminUser))
                {
                    return new RecommendationResponse { Success = false, Message = "لا تملك صلاحية تعديل التوصيات" };
                }

                var recommendation = await _recommendationRepository.GetByIdAsync(request.Id);
                if (recommendation == null)
                {
                    return new RecommendationResponse { Success = false, Message = "التوصية غير موجودة" };
                }

                var fileName = await HandleImageUploadAsync(request.ImageFile, recommendation.ImageUrl);

                recommendation.TitleAr = request.TitleAr;
                recommendation.TitleEn = request.TitleEn;
                recommendation.DescriptionAr = request.DescriptionAr;
                recommendation.DescriptionEn = request.DescriptionEn;
                recommendation.ImageUrl = fileName;
                recommendation.Season = request.Season;
                recommendation.ActivityType = request.ActivityType;
                recommendation.Occasion = request.Occasion;
                recommendation.OccasionEn = request.OccasionEn; // ✅ تحديث المناسبة الإنجليزية
                recommendation.SuggestedPlaceId = request.SuggestedPlaceId;
                recommendation.DisplayOrder = request.DisplayOrder;
                recommendation.IsActive = request.IsActive;
                recommendation.UpdatedAt = DateTime.UtcNow;

                var updated = await _recommendationRepository.UpdateAsync(recommendation);
                var response = MapToFullResponse(updated);
                response.Message = "تم تحديث التوصية بنجاح";

                return response;
            }
            catch (Exception ex)
            {
                return new RecommendationResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        public async Task<BaseResponse> DeleteRecommendationAsync(ClaimsPrincipal adminUser, int id)
        {
            try
            {
                if (!await IsAdminOrSuperAdminAsync(adminUser))
                {
                    return new BaseResponse { Success = false, Message = "لا تملك صلاحية حذف التوصيات" };
                }

                var recommendation = await _recommendationRepository.GetByIdAsync(id);
                if (recommendation == null)
                {
                    return new BaseResponse { Success = false, Message = "التوصية غير موجودة" };
                }

                if (!string.IsNullOrEmpty(recommendation.ImageUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "recommendations", recommendation.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                await _recommendationRepository.DeleteAsync(id);
                return new BaseResponse { Success = true, Message = "تم حذف التوصية بنجاح" };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        public async Task<RecommendationResponse> GetRecommendationByIdAsync(ClaimsPrincipal adminUser, int id)
        {
            try
            {
                if (!await IsAdminOrSuperAdminAsync(adminUser))
                {
                    return new RecommendationResponse { Success = false, Message = "لا تملك صلاحية عرض التوصيات" };
                }

                var recommendation = await _recommendationRepository.GetByIdAsync(id);
                if (recommendation == null)
                {
                    return new RecommendationResponse { Success = false, Message = "التوصية غير موجودة" };
                }

                return MapToFullResponse(recommendation);
            }
            catch (Exception ex)
            {
                return new RecommendationResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        public async Task<RecommendationListResponse> GetAllRecommendationsAsync(ClaimsPrincipal adminUser, RecommendationFilterRequest filter)
        {
            try
            {
                if (!await IsAdminOrSuperAdminAsync(adminUser))
                {
                    return new RecommendationListResponse { Success = false, Message = "لا تملك صلاحية عرض التوصيات" };
                }

                var recommendations = await _recommendationRepository.GetByFiltersAsync(filter);

                var totalCount = recommendations.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                var pagedRecommendations = recommendations
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new RecommendationListResponse
                {
                    Success = true,
                    Message = $"تم جلب {totalCount} توصية",
                    Recommendations = pagedRecommendations.Select(MapToSummaryResponse).ToList(),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new RecommendationListResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        // ==================== Customer Operations ====================

        public async Task<RecommendationResponse> GetActiveRecommendationByIdAsync(int id)
        {
            try
            {
                var recommendation = await _recommendationRepository.GetByIdAsync(id);
                if (recommendation == null || !recommendation.IsActive)
                {
                    return new RecommendationResponse { Success = false, Message = "التوصية غير موجودة" };
                }

                return MapToFullResponse(recommendation);
            }
            catch (Exception ex)
            {
                return new RecommendationResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        public async Task<RecommendationListResponse> GetActiveRecommendationsAsync(RecommendationFilterRequest filter)
        {
            try
            {
                filter.IsActive = true;

                var recommendations = await _recommendationRepository.GetByFiltersAsync(filter);

                var totalCount = recommendations.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

                var pagedRecommendations = recommendations
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new RecommendationListResponse
                {
                    Success = true,
                    Message = $"تم جلب {totalCount} توصية",
                    Recommendations = pagedRecommendations.Select(MapToSummaryResponse).ToList(),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                return new RecommendationListResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        public async Task<RecommendationListResponse> GetRecommendationsBySeasonAsync(RecommendationSeason season, int pageNumber = 1, int pageSize = 10)
        {
            return await GetActiveRecommendationsAsync(new RecommendationFilterRequest
            {
                Season = season,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsActive = true
            });
        }

        public async Task<RecommendationListResponse> GetRecommendationsByActivityAsync(ActivityType activityType, int pageNumber = 1, int pageSize = 10)
        {
            return await GetActiveRecommendationsAsync(new RecommendationFilterRequest
            {
                ActivityType = activityType,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsActive = true
            });
        }
    }
}