using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // ✅ تمت الإضافة
using Microsoft.AspNetCore.WebUtilities;          // ✅ تمت الإضافة
using Microsoft.Extensions.Configuration;         // ✅ تمت الإضافة
using Rovaya.DAL.DTO.Request.Profile;
using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.DTO.Response.Profile;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Identity;
using Rovaya.DAL.Repository.Profile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;                                // ✅ تمت الإضافة
using System.Threading.Tasks;

namespace Rovaya.BLL.Service.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;       // ✅ تمت الإضافة
        private readonly IConfiguration _configuration;   // ✅ تمت الإضافة

        public ProfileService(
            IProfileRepository profileRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IEmailSender emailSender,           // ✅ تمت الإضافة
            IConfiguration configuration)       // ✅ تمت الإضافة
        {
            _profileRepository = profileRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;
            return await _profileRepository.GetUserByIdAsync(userId);
        }

        // ==================== Profile Basics ====================

        public async Task<ProfileResponse> GetFullProfileAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new ProfileResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            return new ProfileResponse
            {
                Success = true,
                Message = "تم جلب البيانات بنجاح",
                UserId = currentUser.Id,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Email = currentUser.Email,
                Country = currentUser.Country,
                PhoneNumber = currentUser.PhoneNumber,
                AdditionalPhoneNumbers = currentUser.AdditionalPhoneNumbers,
                EmergencyNumbers = currentUser.EmergencyNumbers,
                ProfileImageUrl = currentUser.ProfileImageUrl,
                CreatedAt = currentUser.CreatedAt,
                MedicalProfile = currentUser.MedicalProfile != null ? new MedicalProfileResponse
                {
                    Id = currentUser.MedicalProfile.Id,
                    ChronicDiseases = currentUser.MedicalProfile.ChronicDiseases,
                    CurrentMedications = currentUser.MedicalProfile.CurrentMedications,
                    Allergies = currentUser.MedicalProfile.Allergies,
                    LastUpdated = currentUser.MedicalProfile.LastUpdated
                } : null
            };
        }

        public async Task<BaseResponse> UpdateBasicProfileAsync(ClaimsPrincipal user, UpdateBasicProfileRequest request)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            // ✅ التحقق مما إذا كان البريد الإلكتروني قد تغير
            if (currentUser.Email != request.Email)
            {
                // التأكد من أن البريد الجديد غير مستخدم من قبل مستخدم آخر
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new BaseResponse { Success = false, Message = "هذا البريد الإلكتروني مستخدم بالفعل من قبل حساب آخر" };
                }

                // تحديث البريد واسم المستخدم
                currentUser.Email = request.Email;
                currentUser.UserName = request.Email;

                // ✅ إعادة تعيين حالة التأكيد للأمان (سيُطلب منه تأكيد البريد الجديد)
                currentUser.EmailConfirmed = false;

                // ✅ إرسال رابط تأكيد للبريد الجديد
                await SendEmailConfirmationAsync(currentUser);
            }

            currentUser.FirstName = request.FirstName;
            currentUser.LastName = request.LastName;
            currentUser.Country = request.Country;
            currentUser.PhoneNumber = request.PhoneNumber;

            if (request.ProfileImageUrl != null)
            {
                currentUser.ProfileImageUrl = request.ProfileImageUrl;
            }

            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse
            {
                Success = true,
                Message = "تم تحديث البيانات الأساسية بنجاح. تم إرسال رابط تأكيد إلى بريدك الإلكتروني الجديد."
            };
        }

        // ==================== Image Upload ====================

        public async Task<UploadImageResponse> UploadProfileImageAsync(ClaimsPrincipal user, IFormFile file)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new UploadImageResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            if (file == null || file.Length == 0)
            {
                return new UploadImageResponse { Success = false, Message = "لم يتم اختيار ملف" };
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return new UploadImageResponse { Success = false, Message = "نوع الملف غير مدعوم. يرجى استخدام JPG, PNG, أو WEBP." };
            }

            if (file.Length > 2 * 1024 * 1024) // 2 MB
            {
                return new UploadImageResponse { Success = false, Message = "حجم الملف يتجاوز الحد المسموح به (2 ميجابايت)." };
            }

            // حذف الصورة القديمة إذا وجدت لتوفير مساحة السيرفر
            if (!string.IsNullOrEmpty(currentUser.ProfileImageUrl) && currentUser.ProfileImageUrl.StartsWith("/uploads/profiles/"))
            {
                var oldFileName = currentUser.ProfileImageUrl.Replace("/uploads/profiles/", "");
                var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles", oldFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // حفظ الصورة الجديدة
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/profiles/{uniqueFileName}";

            // تحديث المستخدم برابط الصورة الجديد مباشرة
            currentUser.ProfileImageUrl = imageUrl;
            await _profileRepository.UpdateUserAsync(currentUser);

            return new UploadImageResponse
            {
                Success = true,
                Message = "تم رفع الصورة بنجاح",
                ImageUrl = imageUrl
            };
        }

        // ==================== Medical Profile ====================

        public async Task<MedicalProfileResponse> GetMedicalProfileAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new MedicalProfileResponse();

            var medicalProfile = await _profileRepository.GetMedicalProfileByUserIdAsync(currentUser.Id);
            if (medicalProfile == null) return new MedicalProfileResponse();

            return new MedicalProfileResponse
            {
                Id = medicalProfile.Id,
                ChronicDiseases = medicalProfile.ChronicDiseases,
                CurrentMedications = medicalProfile.CurrentMedications,
                Allergies = medicalProfile.Allergies,
                LastUpdated = medicalProfile.LastUpdated
            };
        }

        public async Task<BaseResponse> CreateOrUpdateMedicalProfileAsync(ClaimsPrincipal user, MedicalProfileRequest request)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };

            var existingProfile = await _profileRepository.GetMedicalProfileByUserIdAsync(currentUser.Id);

            if (existingProfile == null)
            {
                var newProfile = new UserMedicalProfile
                {
                    ApplicationUserId = currentUser.Id,
                    ChronicDiseases = request.ChronicDiseases,
                    CurrentMedications = request.CurrentMedications,
                    Allergies = request.Allergies,
                    LastUpdated = DateTime.UtcNow
                };
                await _profileRepository.CreateMedicalProfileAsync(newProfile);
            }
            else
            {
                existingProfile.ChronicDiseases = request.ChronicDiseases;
                existingProfile.CurrentMedications = request.CurrentMedications;
                existingProfile.Allergies = request.Allergies;
                existingProfile.LastUpdated = DateTime.UtcNow;
                await _profileRepository.UpdateMedicalProfileAsync(existingProfile);
            }

            return new BaseResponse { Success = true, Message = "تم حفظ السجل الطبي بنجاح" };
        }

        public async Task<BaseResponse> DeleteMedicalProfileAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };

            await _profileRepository.DeleteMedicalProfileAsync(currentUser.Id);
            return new BaseResponse { Success = true, Message = "تم حذف السجل الطبي بنجاح" };
        }

        // ==================== Emergency Numbers ====================

        public async Task<EmergencyNumbersResponse> GetEmergencyNumbersAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new EmergencyNumbersResponse { Success = false, Message = "المستخدم غير موجود" };

            return new EmergencyNumbersResponse
            {
                Success = true,
                Message = "تم جلب أرقام الطوارئ بنجاح",
                Numbers = currentUser.EmergencyNumbers
            };
        }

        public async Task<BaseResponse> AddEmergencyNumberAsync(ClaimsPrincipal user, EmergencyNumberRequest request)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };

            if (currentUser.EmergencyNumbers == null) currentUser.EmergencyNumbers = new List<string>();

            if (currentUser.EmergencyNumbers.Contains(request.PhoneNumber))
                return new BaseResponse { Success = false, Message = "رقم الطوارئ موجود مسبقاً" };

            if (currentUser.EmergencyNumbers.Count >= 5)
                return new BaseResponse { Success = false, Message = "لا يمكن إضافة أكثر من 5 أرقام طوارئ" };

            currentUser.EmergencyNumbers.Add(request.PhoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse { Success = true, Message = "تم إضافة رقم الطوارئ بنجاح" };
        }

        public async Task<BaseResponse> RemoveEmergencyNumberAsync(ClaimsPrincipal user, string phoneNumber)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };

            if (currentUser.EmergencyNumbers == null || !currentUser.EmergencyNumbers.Contains(phoneNumber))
                return new BaseResponse { Success = false, Message = "رقم الطوارئ غير موجود" };

            currentUser.EmergencyNumbers.Remove(phoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse { Success = true, Message = "تم حذف رقم الطوارئ بنجاح" };
        }

        // ==================== Additional Phone Numbers ====================

        public async Task<BaseResponse> AddAdditionalPhoneNumberAsync(ClaimsPrincipal user, EmergencyNumberRequest request)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };

            if (currentUser.AdditionalPhoneNumbers == null) currentUser.AdditionalPhoneNumbers = new List<string>();

            if (currentUser.AdditionalPhoneNumbers.Contains(request.PhoneNumber))
                return new BaseResponse { Success = false, Message = "رقم الهاتف موجود مسبقاً" };

            if (currentUser.AdditionalPhoneNumbers.Count >= 5)
                return new BaseResponse { Success = false, Message = "لا يمكن إضافة أكثر من 5 أرقام هاتف إضافية" };

            currentUser.AdditionalPhoneNumbers.Add(request.PhoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse { Success = true, Message = "تم إضافة رقم الهاتف الإضافي بنجاح" };
        }

        public async Task<BaseResponse> RemoveAdditionalPhoneNumberAsync(ClaimsPrincipal user, string phoneNumber)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };

            if (currentUser.AdditionalPhoneNumbers == null || !currentUser.AdditionalPhoneNumbers.Contains(phoneNumber))
                return new BaseResponse { Success = false, Message = "رقم الهاتف غير موجود" };

            currentUser.AdditionalPhoneNumbers.Remove(phoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse { Success = true, Message = "تم حذف رقم الهاتف الإضافي بنجاح" };
        }

        // ==================== Admin Methods ====================

        public async Task<ProfileResponse> GetUserProfileByAdminAsync(ClaimsPrincipal adminUser, string targetUserId)
        {
            try
            {
                var adminId = adminUser.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _profileRepository.GetUserByIdAsync(adminId);
                if (admin == null) return new ProfileResponse { Success = false, Message = "غير مصرح" };

                var adminRoles = await _userManager.GetRolesAsync(admin);
                if (!adminRoles.Contains("Admin") && !adminRoles.Contains("SuperAdmin"))
                    return new ProfileResponse { Success = false, Message = "لا تملك صلاحية عرض بيانات المستخدمين" };

                var targetUser = await _profileRepository.GetUserByIdAsync(targetUserId);
                if (targetUser == null) return new ProfileResponse { Success = false, Message = "المستخدم غير موجود" };

                return new ProfileResponse
                {
                    Success = true,
                    Message = "تم جلب بيانات المستخدم بنجاح",
                    UserId = targetUser.Id,
                    FirstName = targetUser.FirstName,
                    LastName = targetUser.LastName,
                    Email = targetUser.Email,
                    Country = targetUser.Country,
                    PhoneNumber = targetUser.PhoneNumber,
                    AdditionalPhoneNumbers = targetUser.AdditionalPhoneNumbers,
                    EmergencyNumbers = targetUser.EmergencyNumbers,
                    ProfileImageUrl = targetUser.ProfileImageUrl,
                    CreatedAt = targetUser.CreatedAt,
                    MedicalProfile = targetUser.MedicalProfile != null ? new MedicalProfileResponse
                    {
                        Id = targetUser.MedicalProfile.Id,
                        ChronicDiseases = targetUser.MedicalProfile.ChronicDiseases,
                        CurrentMedications = targetUser.MedicalProfile.CurrentMedications,
                        Allergies = targetUser.MedicalProfile.Allergies,
                        LastUpdated = targetUser.MedicalProfile.LastUpdated
                    } : null
                };
            }
            catch (Exception ex)
            {
                return new ProfileResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        public async Task<AllUsersResponse> GetAllUsersAsync(ClaimsPrincipal adminUser)
        {
            try
            {
                var adminId = adminUser.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _profileRepository.GetUserByIdAsync(adminId);
                if (admin == null) return new AllUsersResponse { Success = false, Message = "غير مصرح" };

                var adminRoles = await _userManager.GetRolesAsync(admin);
                if (!adminRoles.Contains("Admin") && !adminRoles.Contains("SuperAdmin"))
                    return new AllUsersResponse { Success = false, Message = "لا تملك صلاحية عرض قائمة المستخدمين" };

                var allUsers = await _profileRepository.GetAllUsersAsync();
                var usersList = new List<UserSummaryResponse>();

                foreach (var user in allUsers)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    usersList.Add(new UserSummaryResponse
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Country = user.Country,
                        CreatedAt = user.CreatedAt,
                        Status = user.Status,
                        Roles = userRoles.ToList(),
                        VerificationStatus = user.VerificationStatus
                    });
                }

                return new AllUsersResponse
                {
                    Success = true,
                    Message = $"تم جلب {usersList.Count} مستخدم بنجاح",
                    Users = usersList,
                    TotalCount = usersList.Count
                };
            }
            catch (Exception ex)
            {
                return new AllUsersResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }

        // ==================== Email Confirmation Helpers ====================

        // ✅ دالة جديدة لإرسال رابط تأكيد البريد الإلكتروني
        private async Task SendEmailConfirmationAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var baseUrl = _configuration["AppUrl"] ?? "https://localhost:7194";
            var confirmationLink = $"{baseUrl}/api/auth/account/ConfirmEmail?userId={user.Id}&token={encodedToken}";

            // استخدام نفس القالب المستخدم في التسجيل
            var emailBody = BuildConfirmationEmailTemplate(user.FirstName, confirmationLink);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email | تأكيد بريدك الإلكتروني - Rovaya", emailBody);
        }

        // ✅ قالب تأكيد البريد الإلكتروني (نفسه المستخدم في AuthenticationService)
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
                        <p class='text'>يرجى الضغط على الزر أدناه لتأكيد بريدك الإلكتروني الجديد والبدء في استخدام منصة Rovaya.</p>
                    </div>
                    <div style='text-align: center; margin: 20px 0;'>
                        <a href='{confirmationLink}' class='btn'>تأكيد البريد الإلكتروني</a>
                    </div>
                    <div class='section ltr'>
                        <div class='title'>Welcome, {firstName}! 👋</div>
                        <p class='text'>Please click the button above to confirm your new email address.</p>
                    </div>
                    <div class='footer'>&copy; {DateTime.UtcNow.Year} Rovaya. All rights reserved.</div>
                </div>
            </body></html>";
        }
    }
}