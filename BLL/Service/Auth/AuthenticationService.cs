using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Rovaya.DAL.DTO.Request.Auth;
using Rovaya.DAL.DTO.Response.Auth;
using Rovaya.DAL.Models.Auth;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rovaya.BLL.Service.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
                if (existingUser != null)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "البريد الإلكتروني مسجل مسبقاً"
                    };
                }

                var user = new ApplicationUser
                {
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    Email = registerRequest.Email,
                    UserName = registerRequest.Email,
                    PhoneNumber = registerRequest.PhoneNumber,
                    Country = registerRequest.Country,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, registerRequest.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = $"فشل في إنشاء الحساب: {errors}"
                    };
                }

                await _userManager.AddToRoleAsync(user, "Customer");

                return new RegisterResponse
                {
                    Success = true,
                    Message = "تم إنشاء الحساب بنجاح",
                    UserId = user.Id, // مفيد للـ Frontend
                    Email = user.Email
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = $"حدث خطأ غير متوقع أثناء التسجيل: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user is null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
                    };
                }

                var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
                if (!result)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
                    };
                }

                // جلب أدوار المستخدم لإعادتها للـ Frontend (مهم جداً للتوجيه)
                var roles = await _userManager.GetRolesAsync(user);

                return new LoginResponse
                {
                    Success = true,
                    Message = "تم تسجيل الدخول بنجاح",
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    AccessToken = await GenerateAccessToken(user),
                    Roles = roles.ToList() // تأكد من إضافة List<string> Roles في LoginResponse DTO

                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"حدث خطأ غير متوقع أثناء تسجيل الدخول: {ex.Message}"
                };
            }
        }



        private async  Task<string> GenerateAccessToken(ApplicationUser user) 
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userClaims = new List<Claim>()
{
    new Claim(ClaimTypes.Name, user.UserName),        
    new Claim(ClaimTypes.NameIdentifier, user.Id),      
    new Claim(ClaimTypes.Email, user.Email),           
    new Claim("FirstName", user.FirstName),             
    new Claim("Country", user.Country)                  
};

            foreach (var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }


            var key = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. إنشاء الـ Token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.UtcNow.AddDays(7), // صلاحية لمدة أسبوع
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);




        }








    }
}