using Rovaya.DAL.Models.TourismGuide;

namespace Rovaya.DAL.Repository.TourismGuid
{
    public interface ITourismGuidRepository
    {
        // ================= Categories =================
        Task<IEnumerable<TourismGuideCategory>> GetAllCategoriesAsync(string? langCode);
        Task<TourismGuideCategory?> GetCategoryByIdAsync(int id, string? langCode);
        Task<TourismGuideCategory> CreateCategoryAsync(TourismGuideCategory category);
        Task<bool> UpdateCategoryAsync(TourismGuideCategory category);
        Task<bool> DeleteCategoryAsync(int id);

        // ================= Places =================
        Task<IEnumerable<TourismGuidePlace>> GetAllPlacesAsync(string? langCode, int? categoryId, string? searchName);
        Task<TourismGuidePlace?> GetPlaceByIdAsync(int id, string? langCode);
        Task<TourismGuidePlace> CreatePlaceAsync(TourismGuidePlace place);
        Task<bool> UpdatePlaceAsync(TourismGuidePlace place);
        Task<bool> DeletePlaceAsync(int id);

        // City Sections
        Task<IEnumerable<CitySection>> GetSectionsByCityIdAsync(int cityId);
        Task<CitySection?> GetSectionByIdAsync(int id);
        Task<CitySection> CreateSectionAsync(CitySection section);
        Task<bool> UpdateSectionAsync(CitySection section);
        Task<bool> DeleteSectionAsync(int id);

        // Cities
        Task<IEnumerable<TourismGuidePlace>> GetAllCitiesAsync(string? langCode, int? categoryId, string? searchName);
        Task<IEnumerable<TourismGuidePlace>> GetPlacesByParentIdAsync(int parentId, string? langCode);

    }
}