using Rovaya.DAL.DTO.Request.Auth;
using Rovaya.DAL.DTO.Response.Auth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rovaya.BLL.Service.Auth
{
    public interface IAuthenticationService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest);

        Task<BaseResponse> ResendConfirmationEmailAsync(string email);
        Task<bool> ConfirmEmailAsync(string userId, string token);

        Task<ForgetPasswordResponse> RequestPasswordReset(ForgetPasswordRequest request);
        Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request);
        Task<ChangePasswordResponse> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequest request);


        Task<DeleteAccountResponse> DeleteAccountAsync(ClaimsPrincipal user, DeleteAccountRequest request);
        Task<CreateEmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<BlockUserResponse> BlockUserAsync(ClaimsPrincipal callerUser, BlockUserRequest request);
        Task<BlockUserResponse> UnblockUserAsync(ClaimsPrincipal callerUser, string targetUserId);


    }

}