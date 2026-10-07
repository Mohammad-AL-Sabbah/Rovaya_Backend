using Rovaya.DAL.DTO.Request.Profile;
using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.DTO.Response.Profile;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Profile
{
    public interface IProfileService
    {
        Task<ProfileResponse> GetFullProfileAsync(ClaimsPrincipal user);
        Task<BaseResponse> UpdateBasicProfileAsync(ClaimsPrincipal user, UpdateBasicProfileRequest request);
        Task<ProfileResponse> GetUserProfileByAdminAsync(ClaimsPrincipal adminUser, string targetUserId);
        Task<MedicalProfileResponse> GetMedicalProfileAsync(ClaimsPrincipal user);
        Task<BaseResponse> CreateOrUpdateMedicalProfileAsync(ClaimsPrincipal user, MedicalProfileRequest request);
        Task<BaseResponse> DeleteMedicalProfileAsync(ClaimsPrincipal user);
        Task<AllUsersResponse> GetAllUsersAsync(ClaimsPrincipal adminUser);
        Task<EmergencyNumbersResponse> GetEmergencyNumbersAsync(ClaimsPrincipal user);
        Task<BaseResponse> AddEmergencyNumberAsync(ClaimsPrincipal user, EmergencyNumberRequest request);
        Task<BaseResponse> RemoveEmergencyNumberAsync(ClaimsPrincipal user, string phoneNumber);

        Task<BaseResponse> AddAdditionalPhoneNumberAsync(ClaimsPrincipal user, EmergencyNumberRequest request);
        Task<BaseResponse> RemoveAdditionalPhoneNumberAsync(ClaimsPrincipal user, string phoneNumber);
    }
}