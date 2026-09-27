using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rovaya.BLL.Service.Auth;
using Rovaya.DAL.DTO.Request.Auth;
using Rovaya.DAL.DTO.Response.Auth;
using System.ComponentModel.DataAnnotations;
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


    

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var isConfirmed = await _authenticationService.ConfirmEmailAsync(userId, token);

            if (isConfirmed)
            {
                var successHtml = @"
                <!DOCTYPE html>
                <html lang='ar' dir='rtl'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>تم تأكيد البريد | Rovaya</title>
                    <style>
                        body { font-family: 'Segoe UI', Tahoma, Arial, sans-serif; background-color: #f4f6f4; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; }
                        .card { background: #ffffff; padding: 40px; border-radius: 20px; text-align: center; max-width: 480px; width: 90%; box-shadow: 0 15px 35px rgba(18, 62, 52, 0.1); border-top: 6px solid #2ECC71; }
                        .icon-circle { width: 80px; height: 80px; background-color: #e8f8f0; color: #2ECC71; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 40px; margin: 0 auto 20px auto; }
                        h1 { color: #123E34; font-size: 24px; margin-bottom: 12px; }
                        p { color: #666; font-size: 15px; line-height: 1.6; margin-bottom: 25px; }
                        .btn { display: inline-block; padding: 13px 32px; background-color: #123E34; color: #ffffff !important; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 15px; border: 2px solid #2ECC71; box-shadow: 0 4px 12px rgba(18, 62, 52, 0.2); }
                    </style>
                </head>
                <body>
                    <div class='card'>
                        <div class='icon-circle'>✓</div>
                        <h1>تم تأكيد البريد الإلكتروني بنجاح!</h1>
                        <p>شكرًا لتأكيد حسابك في منصة <strong>Rovaya</strong>. يمكنك الآن تسجيل الدخول لبدء تخطيط رحلتك.</p>
                        <a href='https://rovaya.onrender.com' class='btn'>الانتقال للموقع الإلكتروني</a>
                    </div>
                </body>
                </html>";

                return Content(successHtml, "text/html");
            }

            var errorHtml = @"
            <!DOCTYPE html>
            <html lang='ar' dir='rtl'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>فشل التأكيد | Rovaya</title>
                <style>
                    body { font-family: 'Segoe UI', Tahoma, Arial, sans-serif; background-color: #f4f6f4; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; }
                    .card { background: #ffffff; padding: 40px; border-radius: 20px; text-align: center; max-width: 480px; width: 90%; box-shadow: 0 15px 35px rgba(0,0,0,0.08); border-top: 6px solid #e74c3c; }
                    .icon-circle { width: 80px; height: 80px; background-color: #fdeaea; color: #e74c3c; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 40px; margin: 0 auto 20px auto; }
                    h1 { color: #c0392b; font-size: 24px; margin-bottom: 12px; }
                    p { color: #666; font-size: 15px; line-height: 1.6; }
                </style>
            </head>
            <body>
                <div class='card'>
                    <div class='icon-circle'>✕</div>
                    <h1>انتهت صلاحية الرابط أو تم التفعيل مسبقاً</h1>
                    <p>الرابط المستخدم غير صالح أو انتهت مدة صلاحيته (5 دقائق). يرجى طلب رابط جديد من صفحة تسجيل الدخول.</p>
                </div>
            </body>
            </html>";

            return Content(errorHtml, "text/html");
        }


        [HttpPost("SendCode")]
        public async Task<IActionResult> RequestPasswordReset(ForgetPasswordRequest request)
        {
            var result = await _authenticationService.RequestPasswordReset(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
         }

        [HttpPatch("ResetPassword")]

        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _authenticationService.ResetPassword(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }






        [HttpPost("ResendConfirmationEmail")]
        [EnableRateLimiting("StrictEmailPolicy")] // تطبيق حد الطلبات لحماية السيرفر
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authenticationService.ResendConfirmationEmailAsync(request.Email);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authenticationService.LoginAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }



        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _authenticationService.ChangePasswordAsync(User, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }



        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            var result = await _authenticationService.DeleteAccountAsync(User, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("create-employee")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            var result = await _authenticationService.CreateEmployeeAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        [HttpPost("block-user")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
        {
            var result = await _authenticationService.BlockUserAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("unblock-user")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UnblockUser([FromBody] string targetUserId)
        {
            var result = await _authenticationService.UnblockUserAsync(User, targetUserId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }




    }
}