using Mapster.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Rovaya.DAL.DTO.Request.Auth;
using Rovaya.DAL.DTO.Response.Auth;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Rovaya.DAL.Enums;

namespace Rovaya.BLL.Service.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
                if (existingUser != null)
                {
                    return new RegisterResponse { Success = false, Message = "البريد الإلكتروني مسجل مسبقاً" };
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
                    CreatedAt = DateTime.UtcNow,
                    LastConfirmationEmailSentAt = DateTime.UtcNow // تسجيل وقت الإنشاء كبداية
                };

                var result = await _userManager.CreateAsync(user, registerRequest.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new RegisterResponse { Success = false, Message = $"فشل في إنشاء الحساب: {errors}" };
                }

                await _userManager.AddToRoleAsync(user, "Customer");
                await SendConfirmationEmailInternalAsync(user);

                return new RegisterResponse
                {
                    Success = true,
                    Message = "تم إنشاء الحساب بنجاح. يرجى مراجعة بريدك الإلكتروني لتأكيد الحساب.",
                    UserId = user.Id,
                    Email = user.Email
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Success = false, Message = $"حدث خطأ غير متوقع أثناء التسجيل: {ex.Message}" };
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                return result.Succeeded;
            }
            catch { return false; }
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

            // 1. التحقق من كلمة المرور مباشرة
            var passwordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!passwordValid)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
                };
            }

            // 2. التحقق من تفعيل الحساب وإعادة الإرسال الذكي
            if (!user.EmailConfirmed)
            {
                bool canResend = user.LastConfirmationEmailSentAt == null ||
                                 (DateTime.UtcNow - user.LastConfirmationEmailSentAt.Value).TotalSeconds >= 50;

                if (canResend)
                {
                    user.LastConfirmationEmailSentAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    await SendConfirmationEmailInternalAsync(user);

                    var roles = await _userManager.GetRolesAsync(user);
                    bool isAdmin = roles.Contains("Admin");

                    string message = isAdmin
                        ? "حسابك غير مفعل بعد. تم إعادة إرسال رابط التفعيل. يرجى أيضاً تغيير كلمة المرور المبدئية فور التفعيل."
                        : "حسابك غير مفعل بعد. تم إعادة إرسال رابط التفعيل إلى بريدك الإلكتروني تلقائياً.";

                    return new LoginResponse { Success = false, Message = message };
                }
                else
                {
                    int waitSeconds = 50 - (int)(DateTime.UtcNow - user.LastConfirmationEmailSentAt.Value).TotalSeconds;
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"حسابك غير مفعل بعد. يرجى الانتظار {waitSeconds} ثانية قبل محاولة تسجيل الدخول مرة أخرى."
                    };
                }
            }

            // 3. 🔒 التحقق من حالة الحظر (Block Status Check)
            if (user.Status == AccountStatus.PermanentlyBlocked)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"حسابك محظور بشكل دائم. السبب: {user.BlockReason}. يرجى التواصل مع الإدارة."
                };
            }
            else if (user.Status == AccountStatus.TemporarilyBlocked)
            {
                if (user.BlockExpirationDate.HasValue && user.BlockExpirationDate.Value <= DateTime.UtcNow)
                {
                    // ✅ فك الحظر تلقائياً لانتهاء المدة
                    user.Status = AccountStatus.Active;
                    user.BlockReason = null;
                    user.BlockExpirationDate = null;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    // ❌ الحظر لا يزال سارياً (مع تنسيق ذكي للوقت)
                    var remainingTime = user.BlockExpirationDate.Value - DateTime.UtcNow;

                    string remainingTimeMessage;
                    if (remainingTime.TotalMinutes < 1)
                    {
                        remainingTimeMessage = "أقل من دقيقة";
                    }
                    else if (remainingTime.TotalHours < 1)
                    {
                        remainingTimeMessage = $"{Math.Ceiling(remainingTime.TotalMinutes)} دقيقة";
                    }
                    else if (remainingTime.TotalDays < 1)
                    {
                        remainingTimeMessage = $"{Math.Ceiling(remainingTime.TotalHours)} ساعة";
                    }
                    else
                    {
                        remainingTimeMessage = $"{remainingTime.Days} يوم و {remainingTime.Hours} ساعة";
                    }

                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"حسابك محظور مؤقتاً. السبب: {user.BlockReason}. المدة المتبقية: {remainingTimeMessage}."
                    };
                }
            }

            // 4. التحقق من Lockout (محاولات الدخول الخاطئة المتكررة)
            if (await _userManager.IsLockedOutAsync(user))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "الحساب معطل مؤقتاً بسبب محاولات دخول خاطئة متكررة."
                };
            }

            // 5. نجاح تسجيل الدخول وتوليد الـ Token
            var userRoles = await _userManager.GetRolesAsync(user);

            return new LoginResponse
            {
                Success = true,
                Message = "تم تسجيل الدخول بنجاح",
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                AccessToken = await GenerateAccessToken(user),
                Roles = userRoles.ToList()
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
    public async Task<BaseResponse> ResendConfirmationEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || user.EmailConfirmed)
                {
                    return new BaseResponse { Success = true, Message = "إذا كان هذا البريد مسجلاً وغير مفعل، فقد تم إرسال رابط تأكيد جديد." };
                }

                bool canResend = user.LastConfirmationEmailSentAt == null ||
                                 (DateTime.UtcNow - user.LastConfirmationEmailSentAt.Value).TotalSeconds >= 50;

                if (!canResend)
                {
                    int waitSeconds = 50 - (int)(DateTime.UtcNow - user.LastConfirmationEmailSentAt.Value).TotalSeconds;
                    return new BaseResponse { Success = false, Message = $"يرجى الانتظار {waitSeconds} ثانية قبل طلب رابط جديد." };
                }

                user.LastConfirmationEmailSentAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await SendConfirmationEmailInternalAsync(user);

                return new BaseResponse { Success = true, Message = "تم إرسال رابط تأكيد جديد بنجاح." };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        private async Task SendConfirmationEmailInternalAsync(ApplicationUser user)
        {
            var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            var baseUrl = _configuration["AppUrl"] ?? "https://localhost:7194";
            var confirmationLink = $"{baseUrl}/api/auth/account/ConfirmEmail?userId={user.Id}&token={encodedToken}";
            var emailBody = BuildConfirmationEmailTemplate(user.FirstName, confirmationLink);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email | تأكيد بريدك الإلكتروني - Rovaya", emailBody);
        }

        public async Task<ForgetPasswordResponse> RequestPasswordReset(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                // حماية الخصوصية: لا نكشف إذا كان الإيميل موجوداً أم لا
                return new ForgetPasswordResponse { Success = true, Message = "إذا كان هذا البريد مسجلاً لدينا، فقد تم إرسال كود إعادة التعيين." };
            }

            var random = new Random();
            var code = random.Next(1000, 9990).ToString();

            user.CodeResetPassword = code;
            user.CodeResetPasswordExpiration = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            var emailBody = BuildPasswordResetEmailTemplate(user.FirstName, code);
            await _emailSender.SendEmailAsync(request.Email, "Reset Password Code | كود إعادة تعيين كلمة المرور - Rovaya", emailBody);

            return new ForgetPasswordResponse { Success = true, Message = "تم إرسال كود إعادة التعيين إلى بريدك الإلكتروني." };
        }

        public async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return new ResetPasswordResponse { Success = false, Message = "البريد الإلكتروني غير مسجل." };
            }

            if (user.CodeResetPassword != request.Code)
            {
                return new ResetPasswordResponse { Success = false, Message = "كود إعادة التعيين غير صحيح." };
            }

            if (user.CodeResetPasswordExpiration < DateTime.UtcNow)
            {
                return new ResetPasswordResponse { Success = false, Message = "انتهت صلاحية كود إعادة التعيين. يرجى طلب كود جديد." };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "فشل في تغيير كلمة المرور.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // مسح الكود بعد الاستخدام الناجح
            user.CodeResetPassword = null;
            user.CodeResetPasswordExpiration = null;
            await _userManager.UpdateAsync(user);

            var emailBody = BuildPasswordChangedEmailTemplate(user.FirstName);
            await _emailSender.SendEmailAsync(request.Email, "Password Changed Successfully | تم تغيير كلمة المرور بنجاح - Rovaya", emailBody);

            return new ResetPasswordResponse { Success = true, Message = "تم تغيير كلمة المرور بنجاح." };
        }

        private async Task<string> GenerateAccessToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("Country", user.Country ?? string.Empty)
            };

            foreach (var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ================== Email Templates ==================

        private string BuildConfirmationEmailTemplate(string firstName, string confirmationLink)
        {
            return $@"
            <!DOCTYPE html><html><head><meta charset='UTF-8'><style>
                body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; background-color: #f4f6f4; margin: 0; padding: 0; }}
                .email-container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(18, 62, 52, 0.08); }}
                .header {{ background-color: #123E34; padding: 35px 20px; text-align: center; }}
                .brand-name {{ color: #ffffff; font-size: 32px; font-weight: 800; margin: 0; }}
                .brand-tagline {{ color: #2ECC71; font-size: 13px; margin-top: 6px; }}
                .section {{ padding: 30px; }}
                .rtl {{ direction: rtl; text-align: right; }}
                .ltr {{ direction: ltr; text-align: left; }}
                .title {{ font-size: 20px; font-weight: 700; color: #123E34; margin-bottom: 12px; }}
                .text {{ font-size: 14px; line-height: 1.7; color: #555555; margin-bottom: 15px; }}
                .btn {{ background-color: #123E34; color: #ffffff !important; padding: 14px 36px; text-decoration: none; border-radius: 50px; font-weight: bold; display: inline-block; border: 2px solid #2ECC71; }}
                .footer {{ background-color: #F9FAF7; padding: 20px; text-align: center; font-size: 12px; color: #888888; }}
            </style></head><body>
                <div class='email-container'>
                    <div class='header'><h1 class='brand-name'>ROVAYA</h1><div class='brand-tagline'>اكتشف فلسطين ... بطريقتك</div></div>
                    <div class='section rtl'>
                        <div class='title'>أهلاً بك، {firstName}! 👋</div>
                        <p class='text'>يرجى الضغط على الزر أدناه لتأكيد بريدك الإلكتروني والبدء في استخدام منصة Rovaya.</p>
                    </div>
                    <div style='text-align: center; margin: 20px 0;'>
                        <a href='{confirmationLink}' class='btn'>تأكيد البريد الإلكتروني</a>
                    </div>
                    <div class='section ltr'>
                        <div class='title'>Welcome, {firstName}! 👋</div>
                        <p class='text'>Please click the button above to confirm your email address and start exploring Palestine with Rovaya.</p>
                    </div>
                    <div class='footer'>&copy; {DateTime.UtcNow.Year} Rovaya. All rights reserved.</div>
                </div>
            </body></html>";
        }

        private string BuildPasswordResetEmailTemplate(string firstName, string code)
        {
            return $@"
            <!DOCTYPE html><html><head><meta charset='UTF-8'><style>
                body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; background-color: #f4f6f4; margin: 0; padding: 0; }}
                .email-container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(18, 62, 52, 0.08); }}
                .header {{ background-color: #123E34; padding: 35px 20px; text-align: center; }}
                .brand-name {{ color: #ffffff; font-size: 32px; font-weight: 800; margin: 0; }}
                .section {{ padding: 30px; text-align: center; }}
                .rtl {{ direction: rtl; }}
                .ltr {{ direction: ltr; }}
                .title {{ font-size: 20px; font-weight: 700; color: #123E34; margin-bottom: 12px; }}
                .text {{ font-size: 14px; line-height: 1.7; color: #555555; margin-bottom: 15px; }}
                .code-box {{ background-color: #f0fdf4; border: 2px dashed #2ECC71; color: #123E34; font-size: 28px; font-weight: 800; letter-spacing: 4px; padding: 15px 30px; border-radius: 8px; display: inline-block; margin: 20px 0; }}
                .warning {{ color: #d97706; font-size: 13px; font-weight: 600; }}
                .footer {{ background-color: #F9FAF7; padding: 20px; text-align: center; font-size: 12px; color: #888888; }}
            </style></head><body>
                <div class='email-container'>
                    <div class='header'><h1 class='brand-name'>ROVAYA</h1></div>
                    <div class='section rtl'>
                        <div class='title'>أهلاً بك، {firstName}</div>
                        <p class='text'>لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك. استخدم الكود أدناه لإتمام العملية:</p>
                        <div class='code-box'>{code}</div>
                        <p class='warning'>⚠️ هذا الكود صالح لمدة 5 دقائق فقط. إذا لم تطلب هذا التغيير، يرجى تجاهل هذه الرسالة.</p>
                    </div>
                    <div style='border-top: 1px solid #edf2f7; margin: 0 30px;'></div>
                    <div class='section ltr'>
                        <div class='title'>Hello, {firstName}</div>
                        <p class='text'>We received a request to reset your password. Use the code below to proceed:</p>
                        <div class='code-box'>{code}</div>
                        <p class='warning'>⚠️ This code is valid for 5 minutes only. If you didn't request this, please ignore this email.</p>
                    </div>
                    <div class='footer'>&copy; {DateTime.UtcNow.Year} Rovaya. All rights reserved.</div>
                </div>
            </body></html>";
        }
        private string BuildPasswordChangedEmailTemplate(string firstName)
        {
            return $@"
            <!DOCTYPE html><html><head><meta charset='UTF-8'><style>
                body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; background-color: #f4f6f4; margin: 0; padding: 0; }}
                .email-container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(18, 62, 52, 0.08); }}
                .header {{ background-color: #123E34; padding: 35px 20px; text-align: center; }}
                .brand-name {{ color: #ffffff; font-size: 32px; font-weight: 800; margin: 0; }}
                .section {{ padding: 30px; text-align: center; }}
                .rtl {{ direction: rtl; }}
                .ltr {{ direction: ltr; }}
                .title {{ font-size: 20px; font-weight: 700; color: #123E34; margin-bottom: 12px; }}
                .text {{ font-size: 14px; line-height: 1.7; color: #555555; margin-bottom: 15px; }}
                .success-icon {{ font-size: 48px; margin-bottom: 10px; }}
                .footer {{ background-color: #F9FAF7; padding: 20px; text-align: center; font-size: 12px; color: #888888; }}
            </style></head><body>
                <div class='email-container'>
                    <div class='header'><h1 class='brand-name'>ROVAYA</h1></div>
                    <div class='section rtl'>
                        <div class='success-icon'>✅</div>
                        <div class='title'>تم تغيير كلمة المرور بنجاح</div>
                        <p class='text'>أهلاً بك، {firstName}.<br>نود إعلامك بأنه تم تغيير كلمة المرور الخاصة بحسابك في منصة Rovaya بنجاح.</p>
                        <p class='text' style='color: #dc2626; font-weight: 600;'>إذا لم تقم بإجراء هذا التغيير بنفسك، يرجى التواصل مع فريق الدعم فوراً لحماية حسابك.</p>
                    </div>
                    <div style='border-top: 1px solid #edf2f7; margin: 0 30px;'></div>
                    <div class='section ltr'>
                        <div class='success-icon'>✅</div>
                        <div class='title'>Password Changed Successfully</div>
                        <p class='text'>Hello, {firstName}.<br>We are writing to confirm that your password for your Rovaya account has been successfully changed.</p>
                        <p class='text' style='color: #dc2626; font-weight: 600;'>If you did not make this change, please contact our support team immediately to secure your account.</p>
                    </div>
                    <div class='footer'>&copy; {DateTime.UtcNow.Year} Rovaya. All rights reserved.</div>
                </div>
            </body></html>";
        }


        public async Task<ChangePasswordResponse> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequest request)
        {
            try
            {
                // استخراج userId من الـ Token
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "غير مصرح بتنفيذ هذه العملية"
                    };
                }

                // جلب المستخدم
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "المستخدم غير موجود"
                    };
                }

                // التحقق من كلمة المرور الحالية
                var isPasswordValid = await _userManager.CheckPasswordAsync(appUser, request.CurrentPassword);
                if (!isPasswordValid)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "كلمة المرور الحالية غير صحيحة"
                    };
                }

                // منع استخدام نفس كلمة المرور
                if (request.CurrentPassword == request.NewPassword)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "كلمة المرور الجديدة يجب أن تختلف عن كلمة المرور الحالية"
                    };
                }

                // تغيير كلمة المرور
                var changeResult = await _userManager.ChangePasswordAsync(appUser, request.CurrentPassword, request.NewPassword);
                if (!changeResult.Succeeded)
                {
                    var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = $"فشل تغيير كلمة المرور: {errors}"
                    };
                }

                // تحديث Security Stamp لإسقاط الجلسات القديمة
                await _userManager.UpdateSecurityStampAsync(appUser);

                // إرسال إيميل التأكيد
                try
                {
                    var emailBody = BuildPasswordChangedEmailTemplate(appUser.FirstName);
                    await _emailSender.SendEmailAsync(
                        appUser.Email,
                        "Password Changed Successfully | تم تغيير كلمة المرور بنجاح - Rovaya",
                        emailBody
                    );
                }
                catch
                {
                    // نتجاهل خطأ الإيميل لضمان نجاح العملية الأساسية
                }

                return new ChangePasswordResponse
                {
                    Success = true,
                    Message = "تم تغيير كلمة المرور بنجاح، وتم تسجيل الخروج من الأجهزة الأخرى لحماية حسابك."
                };
            }
            catch (Exception ex)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = $"حدث خطأ غير متوقع أثناء تغيير كلمة المرور: {ex.Message}"
                };
            }
        }

        public async Task<DeleteAccountResponse> DeleteAccountAsync(ClaimsPrincipal user, DeleteAccountRequest request)
        {
            try
            {
                // 1. استخراج userId من الـ Token
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return new DeleteAccountResponse
                    {
                        Success = false,
                        Message = "غير مصرح بتنفيذ هذه العملية"
                    };
                }

                // 2. جلب المستخدم
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                {
                    return new DeleteAccountResponse
                    {
                        Success = false,
                        Message = "المستخدم غير موجود"
                    };
                }

                // 3. التحقق من كلمة المرور (خطوة أمنية إلزامية)
                var isPasswordValid = await _userManager.CheckPasswordAsync(appUser, request.Password);
                if (!isPasswordValid)
                {
                    return new DeleteAccountResponse
                    {
                        Success = false,
                        Message = "كلمة المرور غير صحيحة. يرجى إدخال كلمة المرور الصحيحة لتأكيد حذف الحساب."
                    };
                }

                // 4. إرسال إيميل تنبيه قبل الحذف (Try-Catch منفصل)
                try
                {
                    var emailBody = BuildAccountDeletionEmailTemplate(appUser.FirstName);
                    await _emailSender.SendEmailAsync(
                        appUser.Email,
                        "Account Deleted | تم حذف حسابك - Rovaya",
                        emailBody
                    );
                }
                catch
                {
                    // نتجاهل خطأ الإيميل لضمان نجاح الحذف
                }

                // 5. حذف المستخدم (Identity سيتولى حذف البيانات المرتبطة تلقائياً بفضل Cascade)
                var deleteResult = await _userManager.DeleteAsync(appUser);
                if (!deleteResult.Succeeded)
                {
                    var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                    return new DeleteAccountResponse
                    {
                        Success = false,
                        Message = $"فشل في حذف الحساب: {errors}"
                    };
                }

                return new DeleteAccountResponse
                {
                    Success = true,
                    Message = "تم حذف حسابك بنجاح. نأسف لرحيلك!"
                };
            }
            catch (Exception ex)
            {
                return new DeleteAccountResponse
                {
                    Success = false,
                    Message = $"حدث خطأ غير متوقع أثناء حذف الحساب: {ex.Message}"
                };
            }
        }

        private string BuildAccountDeletionEmailTemplate(string firstName)
        {
            return $@"
    <!DOCTYPE html><html><head><meta charset='UTF-8'><style>
        body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; background-color: #f4f6f4; margin: 0; padding: 0; }}
        .email-container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(18, 62, 52, 0.08); }}
        .header {{ background-color: #123E34; padding: 35px 20px; text-align: center; }}
        .brand-name {{ color: #ffffff; font-size: 32px; font-weight: 800; margin: 0; }}
        .section {{ padding: 30px; text-align: center; }}
        .rtl {{ direction: rtl; }}
        .ltr {{ direction: ltr; }}
        .title {{ font-size: 20px; font-weight: 700; color: #123E34; margin-bottom: 12px; }}
        .text {{ font-size: 14px; line-height: 1.7; color: #555555; margin-bottom: 15px; }}
        .warning-icon {{ font-size: 48px; margin-bottom: 10px; }}
        .footer {{ background-color: #F9FAF7; padding: 20px; text-align: center; font-size: 12px; color: #888888; }}
    </style></head><body>
        <div class='email-container'>
            <div class='header'><h1 class='brand-name'>ROVAYA</h1></div>
            <div class='section rtl'>
                <div class='warning-icon'>👋</div>
                <div class='title'>وداعاً، {firstName}</div>
                <p class='text'>نود إعلامك بأنه تم حذف حسابك في منصة Rovaya بنجاح.</p>
                <p class='text'>تم حذف جميع بياناتك الشخصية، بما في ذلك الملف الطبي وأرقام الطوارئ، بشكل نهائي من قواعد بياناتنا.</p>
                <p class='text' style='color: #2ECC71; font-weight: 600;'>نتمنى لك رحلات سعيدة ومغامرات رائعة في فلسطين! 🇵🇸</p>
            </div>
            <div style='border-top: 1px solid #edf2f7; margin: 0 30px;'></div>
            <div class='section ltr'>
                <div class='warning-icon'>👋</div>
                <div class='title'>Goodbye, {firstName}</div>
                <p class='text'>We would like to inform you that your Rovaya account has been successfully deleted.</p>
                <p class='text'>All your personal data, including your medical profile and emergency numbers, has been permanently removed from our databases.</p>
                <p class='text' style='color: #2ECC71; font-weight: 600;'>We wish you happy journeys and wonderful adventures in Palestine! 🇵🇸</p>
            </div>
            <div class='footer'>&copy; {DateTime.UtcNow.Year} Rovaya. All rights reserved.</div>
        </div>
    </body></html>";
        }
        public async Task<CreateEmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            try
            {
                // 1. التحقق من عدم وجود الإيميل مسبقاً
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new CreateEmployeeResponse
                    {
                        Success = false,
                        Message = "هذا البريد الإلكتروني مسجل مسبقاً في النظام"
                    };
                }

                // 2. إنشاء المستخدم (EmailConfirmed = false ليحتاج لتأكيد الإيميل)
                var user = new ApplicationUser
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    UserName = request.Email,
                    Country = request.Country ?? "فلسطين",
                    EmailConfirmed = false, // يحتاج لتأكيد الإيميل
                    CreatedAt = DateTime.UtcNow,
                    LastConfirmationEmailSentAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, request.TemporaryPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return new CreateEmployeeResponse
                    {
                        Success = false,
                        Message = $"فشل في إنشاء الحساب: {errors}"
                    };
                }

                // 3. إضافة دور Admin
                await _userManager.AddToRoleAsync(user, "Admin");

                // 4. إرسال إيميل التأكيد مع تنبيه تغيير كلمة المرور
                await SendEmployeeConfirmationEmailAsync(user, request.TemporaryPassword);

                return new CreateEmployeeResponse
                {
                    Success = true,
                    Message = "تم إنشاء حساب الموظف بنجاح. تم إرسال إيميل للتأكيد مع كلمة المرور المبدئية.",
                    EmployeeId = user.Id
                };
            }
            catch (Exception ex)
            {
                return new CreateEmployeeResponse
                {
                    Success = false,
                    Message = $"حدث خطأ غير متوقع: {ex.Message}"
                };
            }
        }

        private async Task SendEmployeeConfirmationEmailAsync(ApplicationUser user, string temporaryPassword)
        {
            var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            var baseUrl = _configuration["AppUrl"] ?? "https://localhost:7194";
            var confirmationLink = $"{baseUrl}/api/auth/account/ConfirmEmail?userId={user.Id}&token={encodedToken}";

            var emailBody = BuildEmployeeConfirmationEmailTemplate(user.FirstName, confirmationLink, temporaryPassword);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Welcome to Rovaya Team | أهلاً بك في فريق Rovaya",
                emailBody
            );
        }
        private string BuildEmployeeConfirmationEmailTemplate(string firstName, string confirmationLink, string temporaryPassword)
        {
            return $@"
    <!DOCTYPE html><html><head><meta charset='UTF-8'><style>
        body {{ font-family: 'Segoe UI', Tahoma, Arial, sans-serif; background-color: #f4f6f4; margin: 0; padding: 0; }}
        .email-container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(18, 62, 52, 0.08); }}
        .header {{ background-color: #123E34; padding: 35px 20px; text-align: center; }}
        .brand-name {{ color: #ffffff; font-size: 32px; font-weight: 800; margin: 0; }}
        .brand-tagline {{ color: #2ECC71; font-size: 13px; margin-top: 6px; }}
        .section {{ padding: 30px; }}
        .rtl {{ direction: rtl; text-align: right; }}
        .ltr {{ direction: ltr; text-align: left; }}
        .title {{ font-size: 20px; font-weight: 700; color: #123E34; margin-bottom: 12px; }}
        .text {{ font-size: 14px; line-height: 1.7; color: #555555; margin-bottom: 15px; }}
        .btn {{ background-color: #123E34; color: #ffffff !important; padding: 14px 36px; text-decoration: none; border-radius: 50px; font-weight: bold; display: inline-block; border: 2px solid #2ECC71; }}
        .password-box {{ background-color: #f0fdf4; border: 2px dashed #2ECC71; color: #123E34; font-size: 18px; font-weight: 700; padding: 12px 20px; border-radius: 8px; display: inline-block; margin: 10px 0; letter-spacing: 2px; }}
        .warning {{ background-color: #fff3cd; color: #856404; padding: 12px; border-radius: 8px; font-size: 13px; margin-top: 15px; border-left: 4px solid #ffc107; }}
        .footer {{ background-color: #F9FAF7; padding: 20px; text-align: center; font-size: 12px; color: #888888; }}
    </style></head><body>
        <div class='email-container'>
            <div class='header'><h1 class='brand-name'>ROVAYA</h1><div class='brand-tagline'>اكتشف فلسطين ... بطريقتك</div></div>
            
            <div class='section rtl'>
                <div class='title'>أهلاً بك في فريق Rovaya، {firstName}! 🎉</div>
                <p class='text'>تم إنشاء حسابك كمدير في منصة Rovaya بنجاح. يرجى اتباع الخطوات التالية:</p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                    <p style='margin: 5px 0; font-weight: 600; color: #123E34;'>1️⃣ كلمة المرور المبدئية:</p>
                    <div class='password-box'>{temporaryPassword}</div>
                </div>
                
                <p style='margin: 15px 0; font-weight: 600; color: #123E34;'>2️⃣ تأكيد بريدك الإلكتروني:</p>
                <div style='text-align: center; margin: 15px 0;'>
                    <a href='{confirmationLink}' class='btn'>تأكيد البريد الإلكتروني</a>
                </div>
                
                <div class='warning'>
                    <strong>⚠️ مهم جداً:</strong><br>
                    • يرجى تغيير كلمة المرور المبدئية فور تسجيل الدخول لأول مرة<br>
                    • هذا الرابط صالح لمدة 24 ساعة فقط<br>
                    • إذا لم تقم بتأكيد الحساب خلال هذه المدة، يرجى التواصل مع المدير
                </div>
            </div>
            
            <div style='border-top: 1px solid #edf2f7; margin: 0 30px;'></div>
            
            <div class='section ltr'>
                <div class='title'>Welcome to Rovaya Team, {firstName}! 🎉</div>
                <p class='text'>Your account as an Admin has been successfully created. Please follow these steps:</p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                    <p style='margin: 5px 0; font-weight: 600; color: #123E34;'>1️⃣ Temporary Password:</p>
                    <div class='password-box'>{temporaryPassword}</div>
                </div>
                
                <p style='margin: 15px 0; font-weight: 600; color: #123E34;'>2️⃣ Confirm your email:</p>
                <div style='text-align: center; margin: 15px 0;'>
                    <a href='{confirmationLink}' class='btn'>Confirm Email</a>
                </div>
                
                <div class='warning'>
                    <strong>⚠️ Important:</strong><br>
                    • Please change your temporary password immediately after first login<br>
                    • This link is valid for 24 hours only<br>
                    • If you don't confirm within this time, please contact your manager
                </div>
            </div>
            
            <div class='footer'>&copy; {DateTime.UtcNow.Year} Rovaya. All rights reserved.</div>
        </div>
    </body></html>";
        }


        public async Task<BlockUserResponse> BlockUserAsync(ClaimsPrincipal callerUser, BlockUserRequest request)
        {
            try
            {
                // 1. جلب بيانات من يقوم بالحظر
                var callerId = callerUser.FindFirstValue(ClaimTypes.NameIdentifier);
                var caller = await _userManager.FindByIdAsync(callerId);
                if (caller == null)
                    return new BlockUserResponse { Success = false, Message = "غير مصرح بتنفيذ هذه العملية" };

                var callerRoles = await _userManager.GetRolesAsync(caller);
                bool isCallerSuperAdmin = callerRoles.Contains("SuperAdmin");
                bool isCallerAdmin = callerRoles.Contains("Admin");

                // 2. جلب المستخدم المستهدف
                var targetUser = await _userManager.FindByIdAsync(request.TargetUserId);
                if (targetUser == null)
                    return new BlockUserResponse { Success = false, Message = "المستخدم المستهدف غير موجود" };

                var targetRoles = await _userManager.GetRolesAsync(targetUser);
                bool isTargetAdminOrSuper = targetRoles.Contains("Admin") || targetRoles.Contains("SuperAdmin");

                // 3. 🔒 التحقق الأمني الصارم من الصلاحيات
                if (isCallerAdmin && isTargetAdminOrSuper)
                {
                    return new BlockUserResponse
                    {
                        Success = false,
                        Message = "لا يحق للموظف حظر موظف آخر أو مدير نظام. هذه الصلاحية محجوزة للمدير الفائق (SuperAdmin) فقط."
                    };
                }

                if (!isCallerSuperAdmin && !isCallerAdmin)
                {
                    return new BlockUserResponse { Success = false, Message = "لا تملك صلاحية تنفيذ هذا الإجراء" };
                }

                // 4. تطبيق الحظر (المنطق المرن الجديد)
                DateTime? expirationDate = null;
                string durationMsg = "بشكل دائم";

                // التحقق مما إذا كان الحظر مؤقتاً (يوجد قيمة ووحدة قياس)
                if (request.DurationValue.HasValue && request.DurationValue > 0 && request.DurationUnit.HasValue)
                {
                    var now = DateTime.UtcNow;

                    switch (request.DurationUnit.Value)
                    {
                        case BlockDurationUnit.Minutes:
                            expirationDate = now.AddMinutes(request.DurationValue.Value);
                            durationMsg = $"لمدة {request.DurationValue.Value} دقيقة";
                            break;
                        case BlockDurationUnit.Hours:
                            expirationDate = now.AddHours(request.DurationValue.Value);
                            durationMsg = $"لمدة {request.DurationValue.Value} ساعة";
                            break;
                        case BlockDurationUnit.Days:
                            expirationDate = now.AddDays(request.DurationValue.Value);
                            durationMsg = $"لمدة {request.DurationValue.Value} يوم";
                            break;
                        case BlockDurationUnit.Months:
                            expirationDate = now.AddMonths(request.DurationValue.Value);
                            durationMsg = $"لمدة {request.DurationValue.Value} شهر";
                            break;
                        case BlockDurationUnit.Years:
                            expirationDate = now.AddYears(request.DurationValue.Value);
                            durationMsg = $"لمدة {request.DurationValue.Value} سنة";
                            break;
                    }

                    targetUser.Status = AccountStatus.TemporarilyBlocked;
                }
                else
                {
                    // إذا لم يتم تحديد مدة، فهو حظر دائم
                    targetUser.Status = AccountStatus.PermanentlyBlocked;
                }

                targetUser.BlockExpirationDate = expirationDate;
                targetUser.BlockReason = request.Reason;

                await _userManager.UpdateAsync(targetUser);

                // 5. إسقاط جلسات الدخول الحالية للمستخدم المحظور فوراً (أمان عالي)
                await _userManager.UpdateSecurityStampAsync(targetUser);

                return new BlockUserResponse
                {
                    Success = true,
                    Message = $"تم حظر المستخدم {targetUser.Email} {durationMsg} بنجاح. السبب: {request.Reason}"
                };
            }
            catch (Exception ex)
            {
                return new BlockUserResponse
                {
                    Success = false,
                    Message = $"حدث خطأ غير متوقع أثناء حظر المستخدم: {ex.Message}"
                };
            }
        }
        public async Task<BlockUserResponse> UnblockUserAsync(ClaimsPrincipal callerUser, string targetUserId)
        {
            try
            {
                var callerId = callerUser.FindFirstValue(ClaimTypes.NameIdentifier);
                var caller = await _userManager.FindByIdAsync(callerId);
                var callerRoles = await _userManager.GetRolesAsync(caller);

                bool isCallerSuperAdmin = callerRoles.Contains("SuperAdmin");
                bool isCallerAdmin = callerRoles.Contains("Admin");

                var targetUser = await _userManager.FindByIdAsync(targetUserId);
                if (targetUser == null) return new BlockUserResponse { Success = false, Message = "المستخدم غير موجود" };

                var targetRoles = await _userManager.GetRolesAsync(targetUser);
                bool isTargetAdminOrSuper = targetRoles.Contains("Admin") || targetRoles.Contains("SuperAdmin");

                // نفس التحقق الأمني
                if (isCallerAdmin && isTargetAdminOrSuper)
                {
                    return new BlockUserResponse { Success = false, Message = "لا يحق للموظف فك الحظر عن موظف آخر أو مدير." };
                }

                // إعادة الحالة إلى نشط
                targetUser.Status = AccountStatus.Active;
                targetUser.BlockReason = null;
                targetUser.BlockExpirationDate = null;

                await _userManager.UpdateAsync(targetUser);

                return new BlockUserResponse
                {
                    Success = true,
                    Message = $"تم فك الحظر عن المستخدم {targetUser.Email} بنجاح."
                };
            }
            catch (Exception ex)
            {
                return new BlockUserResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }










    }
}