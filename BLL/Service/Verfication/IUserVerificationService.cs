using Rovaya.DAL.DTO.Request.Business;
using Rovaya.DAL.DTO.Response.Business;
using Rovaya.DAL.Enums;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Business
{
    public interface IUserVerificationService
    {
        Task<VerificationRequestDto> SubmitRequestAsync(ClaimsPrincipal user, SubmitVerificationRequestDto dto);
        Task<UserVerificationStatusResponse> GetMyStatusAsync(ClaimsPrincipal user);
        Task<List<VerificationRequestDto>> GetAllPendingForAdminAsync(ClaimsPrincipal adminUser);
        Task<bool> ReviewRequestAsync(ClaimsPrincipal adminUser, int requestId, ReviewVerificationRequestDto dto);

        Task<List<VerificationRequestDto>> GetAllRequestsForAdminAsync(ClaimsPrincipal adminUser, VerificationRequestStatus? status = null);
    }
}