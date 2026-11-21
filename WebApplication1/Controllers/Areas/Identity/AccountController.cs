using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rovaya.BLL.Service.Audit;
using Rovaya.BLL.Service.Auth;
using Rovaya.BLL.Service.Business; // ✅ جديد
using Rovaya.BLL.Service.Profile;
using Rovaya.BLL.Service.SupportTickets;
using Rovaya.DAL.DTO.Request.Auth;
using Rovaya.DAL.DTO.Request.Business; // ✅ جديد
using Rovaya.DAL.DTO.Request.Profile;
using Rovaya.DAL.DTO.Request.Support;
using Rovaya.DAL.DTO.Response.Auth;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Rovaya.PL.Controllers.Areas.Identity
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IProfileService _profileService;
        private readonly IAuditLogService _auditLogService;
        private readonly ISupportTicketService _supportTicketService;
        private readonly IUserVerificationService _verificationService; // ✅
        private readonly IBusinessPlaceService _businessPlaceService;   // ✅
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            IAuthenticationService authenticationService,
            IProfileService profileService,
            IAuditLogService auditLogService,
            ISupportTicketService supportTicketService,
            IUserVerificationService verificationService,   // ✅
            IBusinessPlaceService businessPlaceService,
            UserManager<ApplicationUser> userManager


            )     // ✅
        {
            _authenticationService = authenticationService;
            _profileService = profileService;
            _auditLogService = auditLogService;
            _supportTicketService = supportTicketService;
            _verificationService = verificationService;     // ✅
            _businessPlaceService = businessPlaceService;   // ✅
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authenticationService.RegisterAsync(request);
            if (!result.Success) return BadRequest(result);
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
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _authenticationService.ResetPassword(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("ResendConfirmationEmail")]
        [EnableRateLimiting("StrictEmailPolicy")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authenticationService.ResendConfirmationEmailAsync(request.Email);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authenticationService.LoginAsync(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _authenticationService.ChangePasswordAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            var result = await _authenticationService.DeleteAccountAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("create-employee")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            var result = await _authenticationService.CreateEmployeeAsync(User, request);
            if (!result.Success) return BadRequest(result);
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

        // ==================== Profile Management ====================

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetFullProfile()
        {
            var result = await _profileService.GetFullProfileAsync(User);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("profile/basic")]
        [Authorize]
        public async Task<IActionResult> UpdateBasicProfile([FromBody] UpdateBasicProfileRequest request)
        {
            var result = await _profileService.UpdateBasicProfileAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("profile/medical")]
        [Authorize]
        public async Task<IActionResult> GetMedicalProfile()
        {
            var result = await _profileService.GetMedicalProfileAsync(User);
            return Ok(result);
        }

        [HttpPost("profile/medical")]
        [Authorize]
        public async Task<IActionResult> CreateOrUpdateMedicalProfile([FromBody] MedicalProfileRequest request)
        {
            var result = await _profileService.CreateOrUpdateMedicalProfileAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("profile/medical")]
        [Authorize]
        public async Task<IActionResult> DeleteMedicalProfile()
        {
            var result = await _profileService.DeleteMedicalProfileAsync(User);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("profile/emergency-numbers")]
        [Authorize]
        public async Task<IActionResult> GetEmergencyNumbers()
        {
            var result = await _profileService.GetEmergencyNumbersAsync(User);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("profile/emergency-numbers")]
        [Authorize]
        public async Task<IActionResult> AddEmergencyNumber([FromBody] EmergencyNumberRequest request)
        {
            var result = await _profileService.AddEmergencyNumberAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("profile/emergency-numbers/{phoneNumber}")]
        [Authorize]
        public async Task<IActionResult> RemoveEmergencyNumber(string phoneNumber)
        {
            var result = await _profileService.RemoveEmergencyNumberAsync(User, phoneNumber);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("profile/additional-phones")]
        [Authorize]
        public async Task<IActionResult> AddAdditionalPhoneNumber([FromBody] EmergencyNumberRequest request)
        {
            var result = await _profileService.AddAdditionalPhoneNumberAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("profile/additional-phones/{phoneNumber}")]
        [Authorize]
        public async Task<IActionResult> RemoveAdditionalPhoneNumber(string phoneNumber)
        {
            var result = await _profileService.RemoveAdditionalPhoneNumberAsync(User, phoneNumber);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("users/{targetUserId}/profile")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetUserProfileByAdmin(string targetUserId)
        {
            var result = await _profileService.GetUserProfileByAdminAsync(User, targetUserId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _profileService.GetAllUsersAsync(User);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("audit-logs")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _auditLogService.GetAuditLogsAsync(User, pageNumber, pageSize);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("delete-user-account")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteUserAccount([FromBody] string targetUserId)
        {
            var result = await _authenticationService.DeleteUserAccountByAdminAsync(User, targetUserId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("upload-profile-image")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile file)
        {
            var result = await _profileService.UploadProfileImageAsync(User, file);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("change-role")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeRoleRequest request)
        {
            var result = await _authenticationService.ChangeUserRoleAsync(User, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // ==================== Support Tickets ====================

        [HttpPost("support/create")]
        [Authorize]
        public async Task<IActionResult> CreateSupportTicket([FromForm] CreateTicketRequest request)
        {
            var result = await _supportTicketService.CreateTicketAsync(User, request);
            return Ok(new { success = true, data = result, message = "تم إرسال التذكرة بنجاح" });
        }

        [HttpGet("support/my-tickets")]
        [Authorize]
        public async Task<IActionResult> GetMyTickets()
        {
            var result = await _supportTicketService.GetMyTicketsAsync(User);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("support/all-tickets")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllTickets()
        {
            var result = await _supportTicketService.GetAllTicketsForAdminAsync(User);
            return Ok(new { success = true, data = result });
        }

        [HttpPost("support/update-status")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateTicketStatusRequest request)
        {
            var result = await _supportTicketService.UpdateTicketStatusAsync(User, request);
            return Ok(new { success = true, data = result, message = "تم تحديث حالة التذكرة بنجاح" });
        }

        [HttpDelete("support/delete/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            try
            {
                await _supportTicketService.DeleteTicketAsync(User, id);
                return Ok(new { success = true, message = "تم حذف التذكرة بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ==================== ✅ Business Verification (طلب التوثيق) ====================

        /// <summary>
        /// المستخدم يرسل طلب توثيق حسابه كصاحب عمل
        /// </summary>
        [HttpPost("business-verification/submit")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SubmitVerificationRequest([FromBody] SubmitVerificationRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "بيانات غير صالحة", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

                var result = await _verificationService.SubmitRequestAsync(User, dto);
                return Ok(new
                {
                    success = true,
                    message = "تم إرسال طلب التوثيق بنجاح. سيتم مراجعته من الإدارة.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// المستخدم يجلب حالة حسابه (Verified / Pending / Rejected / NotVerified)
        /// ✅ هذه النقطة تُستخدم في الموبايل لإظهار أو إخفاء فورم إضافة الأماكن
        /// </summary>
        [HttpGet("business-verification/my-status")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyVerificationStatus()
        {
            try
            {
                var result = await _verificationService.GetMyStatusAsync(User);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// الإدارة تجلب كل طلبات التوثيق المعلقة
        /// </summary>
        [HttpGet("business-verification/admin/pending")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetPendingVerificationRequests()
        {
            try
            {
                var result = await _verificationService.GetAllPendingForAdminAsync(User);
                return Ok(new { success = true, data = result, totalCount = result.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// الإدارة تراجع طلب توثيق (موافقة / رفض + سبب)
        /// ✅ عند الموافقة: الحساب يتحول لـ Verified تلقائياً
        /// </summary>
        [HttpPost("business-verification/admin/review/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ReviewVerificationRequest(int id, [FromBody] ReviewVerificationRequestDto dto)
        {
            try
            {
                var success = await _verificationService.ReviewRequestAsync(User, id, dto);
                if (!success) return NotFound(new { success = false, message = "الطلب غير موجود" });

                var msg = dto.Approve
                    ? "تم توثيق الحساب. المستخدم يستطيع الآن إضافة أماكن على الخريطة."
                    : "تم رفض الطلب وإشعار المستخدم بالسبب.";

                return Ok(new { success = true, message = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ==================== ✅ Business Places (الأماكن على الخريطة) ====================

        /// <summary>
        /// إضافة مكان تجاري — مسموح فقط للحسابات Verified
        /// </summary>
        [HttpPost("business-places")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateBusinessPlace([FromForm] CreateBusinessPlaceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "بيانات غير صالحة" });

                var result = await _businessPlaceService.CreateAsync(User, dto);
                return Ok(new { success = true, message = "تمت إضافة المكان بنجاح على الخريطة", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// المستخدم يجلب أماكنه
        /// </summary>
        [HttpGet("business-places/my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyBusinessPlaces()
        {
            var result = await _businessPlaceService.GetMyPlacesAsync(User);
            return Ok(new { success = true, data = result, totalCount = result.Count });
        }

        /// <summary>
        /// جلب كل الأماكن النشطة لعرضها على الخريطة (للموبايل والويب)
        /// </summary>
        [HttpGet("business-places/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllActiveBusinessPlaces()
        {
            var result = await _businessPlaceService.GetAllActiveAsync();
            return Ok(new { success = true, data = result, totalCount = result.Count });
        }

        /// <summary>
        /// تعديل مكان — مسموح فقط لصاحبه الموثّق
        /// </summary>
        [HttpPut("business-places")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateBusinessPlace([FromForm] UpdateBusinessPlaceDto dto)
        {
            try
            {
                var result = await _businessPlaceService.UpdateAsync(User, dto);
                return Ok(new { success = true, message = "تم تحديث المكان", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// حذف مكان — مسموح فقط لصاحبه الموثّق
        /// </summary>
        [HttpDelete("business-places/{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteBusinessPlace(int id)
        {
            try
            {
                var success = await _businessPlaceService.DeleteAsync(User, id);
                if (!success) return NotFound(new { success = false, message = "المكان غير موجود" });
                return Ok(new { success = true, message = "تم حذف المكان" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
      
        [HttpGet("business-verification/admin/all")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllVerificationRequests([FromQuery] string? status = null)
        {
            try
            {
                // 1. تحويل نص الحالة القادم من الـ Query إلى Enum (اختياري)
                VerificationRequestStatus? statusFilter = null;

                if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
                {
                    if (Enum.TryParse<VerificationRequestStatus>(status, true, out var parsed))
                    {
                        statusFilter = parsed;
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "حالة غير صالحة. القيم المسموحة: Pending, Approved, Rejected, أو all"
                        });
                    }
                }

                // 2. استدعاء الخدمة لجلب البيانات مع الفلتر
                var result = await _verificationService.GetAllRequestsForAdminAsync(User, statusFilter);

                // 3. إرجاع الاستجابة بتنسيق موحد
                return Ok(new
                {
                    success = true,
                    data = result,
                    totalCount = result.Count,
                    filter = statusFilter?.ToString() ?? "all"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"حدث خطأ في الخادم: {ex.Message}" });
            }
        }


        [HttpPost("users/{userId}/revoke-verification")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> RevokeVerification(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { success = false, message = "المستخدم غير موجود" });

            if (user.VerificationStatus != UserVerificationStatus.Verified)
            {
                return BadRequest(new { success = false, message = "هذا الحساب ليس موثقاً أصلاً" });
            }

            // ✅ إلغاء التوثيق
            user.VerificationStatus = UserVerificationStatus.NotVerified;
            user.VerifiedAt = null;
            user.VerifiedByAdminId = null;
            user.VerificationNotes = "تم إلغاء التوثيق من قبل الإدارة";

            await _userManager.UpdateAsync(user);

            // تسجيل في الـ Audit
            await _auditLogService.LogActionAsync(
                user: User,
                action: "RevokeUserVerification",
                targetUserId: user.Id,
                targetUserName: $"{user.FirstName} {user.LastName}",
                details: $"تم إلغاء توثيق الحساب كصاحب عمل",
                reason: "إجراء إداري"
            );

            return Ok(new { success = true, message = "تم إلغاء توثيق الحساب بنجاح" });
        }

    }
}