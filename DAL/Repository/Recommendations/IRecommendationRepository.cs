using Rovaya.DAL.DTO.Request.Recommendations;
using Rovaya.DAL.Models.Recommendations;

namespace Rovaya.DAL.Repository.Recommendations
{
    public interface IRecommendationRepository
    {
        Task<RovayaRecommendation?> GetByIdAsync(int id);
        Task<List<RovayaRecommendation>> GetAllAsync();
        Task<List<RovayaRecommendation>> GetActiveAsync();
        Task<List<RovayaRecommendation>> GetByFiltersAsync(RecommendationFilterRequest filter);

        Task<RovayaRecommendation> CreateAsync(RovayaRecommendation recommendation);
        Task<RovayaRecommendation> UpdateAsync(RovayaRecommendation recommendation);
        Task DeleteAsync(int id);

        Task<int> GetTotalCountAsync();
        Task<int> GetActiveCountAsync();
    }
}