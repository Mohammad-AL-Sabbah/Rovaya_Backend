using Rovaya.DAL.DTO.Request.Business;
using Rovaya.DAL.DTO.Response.Business;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Business
{
    public interface IBusinessPlaceService
    {
        Task<BusinessPlaceResponseDto> CreateAsync(ClaimsPrincipal user, CreateBusinessPlaceDto dto);
        Task<List<BusinessPlaceResponseDto>> GetMyPlacesAsync(ClaimsPrincipal user);
        Task<List<BusinessPlaceResponseDto>> GetAllActiveAsync();
        Task<BusinessPlaceResponseDto> UpdateAsync(ClaimsPrincipal user, UpdateBusinessPlaceDto dto);
        Task<bool> DeleteAsync(ClaimsPrincipal user, int id);

        Task<bool> DeleteByAdminAsync(ClaimsPrincipal adminUser, int id);
        Task<bool> ToggleActiveByAdminAsync(ClaimsPrincipal adminUser, int id, bool isActive);




    }
}