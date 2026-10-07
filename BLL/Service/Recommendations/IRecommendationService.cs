using Rovaya.DAL.DTO.Request.Recommendations;
using Rovaya.DAL.DTO.Response.Recommendations;
using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.Enums;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Recommendations
{
    public interface IRecommendationService
    {
        // Admin Operations
        Task<RecommendationResponse> CreateRecommendationAsync(ClaimsPrincipal adminUser, CreateRecommendationRequest request);
        Task<RecommendationResponse> UpdateRecommendationAsync(ClaimsPrincipal adminUser, UpdateRecommendationRequest request);
        Task<BaseResponse> DeleteRecommendationAsync(ClaimsPrincipal adminUser, int id);
        Task<RecommendationResponse> GetRecommendationByIdAsync(ClaimsPrincipal adminUser, int id);
        Task<RecommendationListResponse> GetAllRecommendationsAsync(ClaimsPrincipal adminUser, RecommendationFilterRequest filter);

        // Customer Operations
        Task<RecommendationResponse> GetActiveRecommendationByIdAsync(int id);
        Task<RecommendationListResponse> GetActiveRecommendationsAsync(RecommendationFilterRequest filter);
        Task<RecommendationListResponse> GetRecommendationsBySeasonAsync(RecommendationSeason season, int pageNumber = 1, int pageSize = 10);
        Task<RecommendationListResponse> GetRecommendationsByActivityAsync(ActivityType activityType, int pageNumber = 1, int pageSize = 10);
    }
}