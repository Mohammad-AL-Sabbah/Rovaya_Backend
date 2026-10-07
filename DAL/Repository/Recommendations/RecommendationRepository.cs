using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Data;
using Rovaya.DAL.DTO.Request.Recommendations;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Recommendations;

namespace Rovaya.DAL.Repository.Recommendations
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly ApplicationDbContext _context;

        public RecommendationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RovayaRecommendation?> GetByIdAsync(int id)
        {
            return await _context.RovayaRecommendations
                .Include(r => r.CreatedBy)
                .Include(r => r.SuggestedPlace)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<RovayaRecommendation>> GetAllAsync()
        {
            return await _context.RovayaRecommendations
                .Include(r => r.CreatedBy)
                .Include(r => r.SuggestedPlace)
                .OrderBy(r => r.DisplayOrder)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<RovayaRecommendation>> GetActiveAsync()
        {
            return await _context.RovayaRecommendations
                .Include(r => r.SuggestedPlace)
                .Where(r => r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<RovayaRecommendation>> GetByFiltersAsync(RecommendationFilterRequest filter)
        {
            var query = _context.RovayaRecommendations
                .Include(r => r.SuggestedPlace)
                .AsQueryable();

            if (filter.Season.HasValue)
                query = query.Where(r => r.Season == filter.Season.Value || r.Season == RecommendationSeason.All);

            if (filter.ActivityType.HasValue)
                query = query.Where(r => r.ActivityType == filter.ActivityType.Value);

            if (!string.IsNullOrEmpty(filter.Occasion))
                query = query.Where(r => r.Occasion != null && r.Occasion.Contains(filter.Occasion));

            if (filter.IsActive.HasValue)
                query = query.Where(r => r.IsActive == filter.IsActive.Value);

            return await query
                .OrderBy(r => r.DisplayOrder)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<RovayaRecommendation> CreateAsync(RovayaRecommendation recommendation)
        {
            await _context.RovayaRecommendations.AddAsync(recommendation);
            await _context.SaveChangesAsync();
            return recommendation;
        }

        public async Task<RovayaRecommendation> UpdateAsync(RovayaRecommendation recommendation)
        {
            _context.RovayaRecommendations.Update(recommendation);
            await _context.SaveChangesAsync();
            return recommendation;
        }

        public async Task DeleteAsync(int id)
        {
            var recommendation = await _context.RovayaRecommendations.FindAsync(id);
            if (recommendation != null)
            {
                _context.RovayaRecommendations.Remove(recommendation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.RovayaRecommendations.CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.RovayaRecommendations.CountAsync(r => r.IsActive);
        }
    }
}