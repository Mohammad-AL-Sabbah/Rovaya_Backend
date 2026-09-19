using Rovaya.DAL.DTO.Request.TourismGuid;
using Rovaya.DAL.DTO.Response.TourismGuid;

namespace Rovaya.BLL.Service.TourismGuide
{
    public interface ITourismGuidService
    {
        // ================= Categories =================
        Task<IEnumerable<TourismGuidResponse>> GetAllCategoriesAsync(string langCode);
        Task<TourismGuidResponse?> GetCategoryByIdAsync(int id, string langCode);
        Task<TourismGuidResponse> CreateCategoryAsync(TourismGuidRequest request);
        Task<bool> UpdateCategoryAsync(int id, TourismGuidRequest request);
        Task<bool> DeleteCategoryAsync(int id);

        // ================= Places =================
        Task<IEnumerable<TourismPlaceResponse>> GetAllPlacesAsync(TourismPlaceSearchRequest searchRequest);
        Task<TourismPlaceResponse?> GetPlaceByIdAsync(int id, string langCode);
        Task<TourismPlaceResponse> CreatePlaceAsync(CreateTourismPlaceRequest request);
        Task<bool> UpdatePlaceAsync(UpdateTourismPlaceRequest request);
        Task<bool> DeletePlaceAsync(int id);

        // ================= City Sections =================
        Task<IEnumerable<CitySectionResponse>> GetSectionsByCityIdAsync(int cityId);
        Task<CitySectionResponse?> GetSectionByIdAsync(int id);
        // ================= Cities =================
        Task<IEnumerable<TourismPlaceResponse>> GetAllCitiesAsync(string langCode, int? categoryId, string? searchName);
        Task<CityDetailsResponse?> GetCityDetailsAsync(int id, string langCode);
        Task<CitySectionResponse> CreateSectionAsync(CreateCitySectionRequest request);
        Task<bool> UpdateSectionAsync(UpdateCitySectionRequest request);
        Task<bool> DeleteSectionAsync(int id);
    }
}