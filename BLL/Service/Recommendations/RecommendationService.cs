using Microsoft.AspNetCore.Identity;
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

        public RecommendationService(IRecommendationRepository recommendationRepository, UserManager<ApplicationUser> userManager)
        {
            _recommendationRepository = recommendationRepository;
            _userManager = userManager;
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
                ImageUrl = rec.ImageUrl,
                Season = rec.Season,
                ActivityType = rec.ActivityType,
                Occasion = rec.Occasion,
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
                ImageUrl = rec.ImageUrl,
                Season = rec.Season.ToString(),
                ActivityType = rec.ActivityType.ToString(),
                Occasion = rec.Occasion,
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

                var recommendation = new RovayaRecommendation
                {
                    TitleAr = request.TitleAr,
                    TitleEn = request.TitleEn,
                    DescriptionAr = request.DescriptionAr,
                    DescriptionEn = request.DescriptionEn,
                    ImageUrl = request.ImageUrl,
                    Season = request.Season,
                    ActivityType = request.ActivityType,
                    Occasion = request.Occasion,
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

                recommendation.TitleAr = request.TitleAr;
                recommendation.TitleEn = request.TitleEn;
                recommendation.DescriptionAr = request.DescriptionAr;
                recommendation.DescriptionEn = request.DescriptionEn;
                recommendation.ImageUrl = request.ImageUrl;
                recommendation.Season = request.Season;
                recommendation.ActivityType = request.ActivityType;
                recommendation.Occasion = request.Occasion;
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