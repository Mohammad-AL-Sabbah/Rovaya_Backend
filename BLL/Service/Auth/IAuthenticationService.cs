using Rovaya.DAL.DTO.Request.Auth;
using Rovaya.DAL.DTO.Response.Auth;
using System.Threading.Tasks;

namespace Rovaya.BLL.Service.Auth
{
    public interface IAuthenticationService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest);




    }
}