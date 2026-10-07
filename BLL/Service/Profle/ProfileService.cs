using Microsoft.AspNetCore.Identity;
using Rovaya.DAL.DTO.Request.Profile;
using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.DTO.Response.Profile;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Identity;
using Rovaya.DAL.Repository.Profile;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(IProfileRepository profileRepository ,UserManager<ApplicationUser> userManager)
        {
            _profileRepository = profileRepository;
            _userManager = userManager;
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

            currentUser.FirstName = request.FirstName;
            currentUser.LastName = request.LastName;
            currentUser.Country = request.Country;

            if (request.ProfileImageUrl != null)
            {
                currentUser.ProfileImageUrl = request.ProfileImageUrl;
            }

            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse
            {
                Success = true,
                Message = "تم تحديث البيانات الأساسية بنجاح"
            };
        }

        // ==================== Medical Profile ====================

        public async Task<MedicalProfileResponse> GetMedicalProfileAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new MedicalProfileResponse();
            }

            var medicalProfile = await _profileRepository.GetMedicalProfileByUserIdAsync(currentUser.Id);
            if (medicalProfile == null)
            {
                return new MedicalProfileResponse();
            }

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
            if (currentUser == null)
            {
                return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            var existingProfile = await _profileRepository.GetMedicalProfileByUserIdAsync(currentUser.Id);

            if (existingProfile == null)
            {
                // إنشاء سجل طبي جديد
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
                // تحديث السجل الطبي الموجود
                existingProfile.ChronicDiseases = request.ChronicDiseases;
                existingProfile.CurrentMedications = request.CurrentMedications;
                existingProfile.Allergies = request.Allergies;
                existingProfile.LastUpdated = DateTime.UtcNow;

                await _profileRepository.UpdateMedicalProfileAsync(existingProfile);
            }

            return new BaseResponse
            {
                Success = true,
                Message = "تم حفظ السجل الطبي بنجاح"
            };
        }

        public async Task<BaseResponse> DeleteMedicalProfileAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            await _profileRepository.DeleteMedicalProfileAsync(currentUser.Id);

            return new BaseResponse
            {
                Success = true,
                Message = "تم حذف السجل الطبي بنجاح"
            };
        }

        // ==================== Emergency Numbers ====================

        public async Task<EmergencyNumbersResponse> GetEmergencyNumbersAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new EmergencyNumbersResponse { Success = false, Message = "المستخدم غير موجود" };
            }

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
            if (currentUser == null)
            {
                return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            if (currentUser.EmergencyNumbers == null)
            {
                currentUser.EmergencyNumbers = new List<string>();
            }

            if (currentUser.EmergencyNumbers.Contains(request.PhoneNumber))
            {
                return new BaseResponse { Success = false, Message = "رقم الطوارئ موجود مسبقاً" };
            }

            if (currentUser.EmergencyNumbers.Count >= 5)
            {
                return new BaseResponse { Success = false, Message = "لا يمكن إضافة أكثر من 5 أرقام طوارئ" };
            }

            currentUser.EmergencyNumbers.Add(request.PhoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse
            {
                Success = true,
                Message = "تم إضافة رقم الطوارئ بنجاح"
            };
        }

        public async Task<BaseResponse> RemoveEmergencyNumberAsync(ClaimsPrincipal user, string phoneNumber)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            if (currentUser.EmergencyNumbers == null || !currentUser.EmergencyNumbers.Contains(phoneNumber))
            {
                return new BaseResponse { Success = false, Message = "رقم الطوارئ غير موجود" };
            }

            currentUser.EmergencyNumbers.Remove(phoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse
            {
                Success = true,
                Message = "تم حذف رقم الطوارئ بنجاح"
            };
        }

        // ==================== Additional Phone Numbers ====================

        public async Task<BaseResponse> AddAdditionalPhoneNumberAsync(ClaimsPrincipal user, EmergencyNumberRequest request)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            if (currentUser.AdditionalPhoneNumbers == null)
            {
                currentUser.AdditionalPhoneNumbers = new List<string>();
            }

            if (currentUser.AdditionalPhoneNumbers.Contains(request.PhoneNumber))
            {
                return new BaseResponse { Success = false, Message = "رقم الهاتف موجود مسبقاً" };
            }

            if (currentUser.AdditionalPhoneNumbers.Count >= 5)
            {
                return new BaseResponse { Success = false, Message = "لا يمكن إضافة أكثر من 5 أرقام هاتف إضافية" };
            }

            currentUser.AdditionalPhoneNumbers.Add(request.PhoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse
            {
                Success = true,
                Message = "تم إضافة رقم الهاتف الإضافي بنجاح"
            };
        }

        public async Task<BaseResponse> RemoveAdditionalPhoneNumberAsync(ClaimsPrincipal user, string phoneNumber)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null)
            {
                return new BaseResponse { Success = false, Message = "المستخدم غير موجود" };
            }

            if (currentUser.AdditionalPhoneNumbers == null || !currentUser.AdditionalPhoneNumbers.Contains(phoneNumber))
            {
                return new BaseResponse { Success = false, Message = "رقم الهاتف غير موجود" };
            }

            currentUser.AdditionalPhoneNumbers.Remove(phoneNumber);
            await _profileRepository.UpdateUserAsync(currentUser);

            return new BaseResponse
            {
                Success = true,
                Message = "تم حذف رقم الهاتف الإضافي بنجاح"
            };
        }


        public async Task<ProfileResponse> GetUserProfileByAdminAsync(ClaimsPrincipal adminUser, string targetUserId)
        {
            try
            {
                // 1. التحقق من صلاحيات الـ Admin
                var adminId = adminUser.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _profileRepository.GetUserByIdAsync(adminId);
                if (admin == null)
                {
                    return new ProfileResponse { Success = false, Message = "غير مصرح" };
                }

                var adminRoles = await _userManager.GetRolesAsync(admin);
                bool isAdminOrSuper = adminRoles.Contains("Admin") || adminRoles.Contains("SuperAdmin");

                if (!isAdminOrSuper)
                {
                    return new ProfileResponse { Success = false, Message = "لا تملك صلاحية عرض بيانات المستخدمين" };
                }

                // 2. جلب بيانات المستخدم المستهدف
                var targetUser = await _profileRepository.GetUserByIdAsync(targetUserId);
                if (targetUser == null)
                {
                    return new ProfileResponse { Success = false, Message = "المستخدم غير موجود" };
                }

                // 3. إرجاع البروفايل الكامل
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
                return new ProfileResponse
                {
                    Success = false,
                    Message = $"حدث خطأ: {ex.Message}"
                };
            }
        }



        public async Task<AllUsersResponse> GetAllUsersAsync(ClaimsPrincipal adminUser)
        {
            try
            {
                // 1. التحقق من صلاحيات الـ Admin
                var adminId = adminUser.FindFirstValue(ClaimTypes.NameIdentifier);
                var admin = await _profileRepository.GetUserByIdAsync(adminId);
                if (admin == null)
                {
                    return new AllUsersResponse { Success = false, Message = "غير مصرح" };
                }

                var adminRoles = await _userManager.GetRolesAsync(admin);
                bool isAdminOrSuper = adminRoles.Contains("Admin") || adminRoles.Contains("SuperAdmin");

                if (!isAdminOrSuper)
                {
                    return new AllUsersResponse { Success = false, Message = "لا تملك صلاحية عرض قائمة المستخدمين" };
                }

                // 2. جلب جميع المستخدمين
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
                        Roles = userRoles.ToList()
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
                return new AllUsersResponse
                {
                    Success = false,
                    Message = $"حدث خطأ: {ex.Message}"
                };
            }
        }



    }
}