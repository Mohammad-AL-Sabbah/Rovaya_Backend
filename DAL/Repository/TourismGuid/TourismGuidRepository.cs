using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Data;
using Rovaya.DAL.Models.TourismGuide;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rovaya.DAL.Repository.TourismGuid
{
    public class TourismGuidRepository : ITourismGuidRepository
    {
        private readonly ApplicationDbContext _context;

        public TourismGuidRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= Categories =================

        public async Task<IEnumerable<TourismGuideCategory>> GetAllCategoriesAsync(string? langCode)
        {
            if (string.IsNullOrWhiteSpace(langCode))
            {
                return await _context.TourismGuideCategories
                    .Include(c => c.TourismGuideTranslations)
                    .ToListAsync();
            }

            return await _context.TourismGuideCategories
                .Include(c => c.TourismGuideTranslations.Where(t => t.LanguageCode == langCode))
                .ToListAsync();
        }

        public async Task<TourismGuideCategory?> GetCategoryByIdAsync(int id, string? langCode)
        {
            if (string.IsNullOrWhiteSpace(langCode))
            {
                return await _context.TourismGuideCategories
                    .Include(c => c.TourismGuideTranslations)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }

            return await _context.TourismGuideCategories
                .Include(c => c.TourismGuideTranslations.Where(t => t.LanguageCode == langCode))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<TourismGuideCategory>> GetCategoriesByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.TourismGuideCategories
                .Include(c => c.TourismGuideTranslations) // ✅ مهم لجلب الأسماء
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<TourismGuideCategory> CreateCategoryAsync(TourismGuideCategory category)
        {
            await _context.TourismGuideCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(TourismGuideCategory category)
        {
            _context.TourismGuideCategories.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.TourismGuideCategories.FindAsync(id);
            if (category == null) return false;

            _context.TourismGuideCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        // ================= Places =================

        public async Task<IEnumerable<TourismGuidePlace>> GetAllPlacesAsync(string? langCode, int? categoryId, string? searchName)
        {
            var query = _context.TourismGuidePlaces.AsQueryable();

            if (string.IsNullOrWhiteSpace(langCode))
            {
                query = query.Include(p => p.Translations)
                             .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations); // ✅ الإصلاح هنا
            }
            else
            {
                query = query.Include(p => p.Translations.Where(t => t.LanguageCode == langCode))
                             .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations); // ✅ والإصلاح هنا
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(p => p.Translations.Any(t => t.Name.Contains(searchName)));

            return await query.ToListAsync();
        }

        public async Task<TourismGuidePlace?> GetPlaceByIdAsync(int id, string? langCode)
        {
            if (string.IsNullOrWhiteSpace(langCode))
            {
                return await _context.TourismGuidePlaces
                    .Include(p => p.Translations)
                    .Include(p => p.SubPlaces).ThenInclude(sp => sp.Translations)
                    .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations) // ✅ الإصلاح هنا
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            return await _context.TourismGuidePlaces
                .Include(p => p.Translations.Where(t => t.LanguageCode == langCode))
                .Include(p => p.SubPlaces).ThenInclude(sp => sp.Translations.Where(t => t.LanguageCode == langCode))
                .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations) // ✅ والإصلاح هنا
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<TourismGuidePlace> CreatePlaceAsync(TourismGuidePlace place)
        {
            await _context.TourismGuidePlaces.AddAsync(place);
            await _context.SaveChangesAsync();
            return place;
        }

        public async Task<bool> UpdatePlaceAsync(TourismGuidePlace place)
        {
            _context.TourismGuidePlaces.Update(place);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlaceAsync(int id)
        {
            var place = await _context.TourismGuidePlaces.FindAsync(id);
            if (place == null) return false;

            _context.TourismGuidePlaces.Remove(place);
            await _context.SaveChangesAsync();
            return true;
        }

        // ================= City Sections =================

        public async Task<IEnumerable<CitySection>> GetSectionsByCityIdAsync(int cityId)
        {
            return await _context.CitySections
                .Where(s => s.CityId == cityId)
                .OrderBy(s => s.Order)
                .ToListAsync();
        }

        public async Task<CitySection?> GetSectionByIdAsync(int id)
        {
            return await _context.CitySections.FindAsync(id);
        }

        public async Task<CitySection> CreateSectionAsync(CitySection section)
        {
            await _context.CitySections.AddAsync(section);
            await _context.SaveChangesAsync();
            return section;
        }

        public async Task<bool> UpdateSectionAsync(CitySection section)
        {
            var existing = await _context.CitySections.FindAsync(section.Id);
            if (existing == null) return false;

            existing.Title = section.Title;
            existing.TitleEn = section.TitleEn;           // ✅ أضفنا هذا
            existing.Content = section.Content;
            existing.ContentEn = section.ContentEn;       // ✅ وأضفنا هذا
            existing.Order = section.Order;
            existing.ImagesJson = section.ImagesJson;
            existing.VideosJson = section.VideosJson;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSectionAsync(int id)
        {
            var section = await _context.CitySections.FindAsync(id);
            if (section == null) return false;

            _context.CitySections.Remove(section);
            await _context.SaveChangesAsync();
            return true;
        }

        // ================= Cities =================

        public async Task<IEnumerable<TourismGuidePlace>> GetAllCitiesAsync(string? langCode, int? categoryId, string? searchName)
        {
            var query = _context.TourismGuidePlaces
                .Where(p => p.ParentPlaceId == null) // المدن فقط
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(langCode))
            {
                query = query.Include(p => p.Translations)
                             .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations); // ✅ الإصلاح هنا
            }
            else
            {
                query = query.Include(p => p.Translations.Where(t => t.LanguageCode == langCode))
                             .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations); // ✅ والإصلاح هنا
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(p => p.Translations.Any(t => t.Name.Contains(searchName)));

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<TourismGuidePlace>> GetPlacesByParentIdAsync(int parentId, string? langCode)
        {
            var query = _context.TourismGuidePlaces
                .Where(p => p.ParentPlaceId == parentId)
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(langCode))
            {
                query = query.Include(p => p.Translations)
                             .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations); // ✅ الإصلاح هنا
            }
            else
            {
                query = query.Include(p => p.Translations.Where(t => t.LanguageCode == langCode))
                             .Include(p => p.Categories).ThenInclude(c => c.TourismGuideTranslations); // ✅ والإصلاح هنا
            }

            return await query.ToListAsync();
        }
    }
}