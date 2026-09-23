using Microsoft.AspNetCore.Mvc;
using Rovaya.BLL.Service.Auth;
using Rovaya.DAL.DTO.Request.Auth;
using System.Threading.Tasks;

namespace Rovaya.PL.Controllers.Areas.Identity
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authenticationService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authenticationService.LoginAsync(request);
           if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }











    }
}