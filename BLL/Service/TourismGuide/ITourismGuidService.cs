using Rovaya.DAL.DTO.Request.TourismGuid;
using Rovaya.DAL.DTO.Response.TourismGuid;
using System.Security.Claims;

namespace Rovaya.BLL.Service.TourismGuide
{
    public interface ITourismGuidService
    {
        // ================= Categories =================
        Task<IEnumerable<TourismGuidResponse>> GetAllCategoriesAsync(string langCode);
        Task<TourismGuidResponse?> GetCategoryByIdAsync(int id, string langCode);
        // ================= Places =================
        Task<IEnumerable<TourismPlaceResponse>> GetAllPlacesAsync(TourismPlaceSearchRequest searchRequest);
        Task<TourismPlaceResponse?> GetPlaceByIdAsync(int id, string langCode);
        // ================= City Sections =================
        Task<IEnumerable<CitySectionResponse>> GetSectionsByCityIdAsync(int cityId);
        Task<CitySectionResponse?> GetSectionByIdAsync(int id);
        // ================= Cities =================
        Task<IEnumerable<TourismPlaceResponse>> GetAllCitiesAsync(string langCode, int? categoryId, string? searchName);
        Task<CityDetailsResponse?> GetCityDetailsAsync(int id, string langCode);

        Task<TourismGuidResponse> CreateCategoryAsync(ClaimsPrincipal user, TourismGuidRequest request);
        Task<bool> UpdateCategoryAsync(ClaimsPrincipal user, int id, TourismGuidRequest request);
        Task<bool> DeleteCategoryAsync(ClaimsPrincipal user, int id);

        Task<TourismPlaceResponse> CreatePlaceAsync(ClaimsPrincipal user, CreateTourismPlaceRequest request);
        Task<bool> UpdatePlaceAsync(ClaimsPrincipal user, UpdateTourismPlaceRequest request);
        Task<bool> DeletePlaceAsync(ClaimsPrincipal user, int id);

        Task<CitySectionResponse> CreateSectionAsync(ClaimsPrincipal user, CreateCitySectionRequest request);
        Task<bool> UpdateSectionAsync(ClaimsPrincipal user, UpdateCitySectionRequest request);
        Task<bool> DeleteSectionAsync(ClaimsPrincipal user, int id);


    }
}