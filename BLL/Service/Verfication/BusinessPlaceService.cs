using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Rovaya.BLL.Service.Audit;
using Rovaya.DAL.DTO.Request.Business;
using Rovaya.DAL.DTO.Response.Business;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Business;
using Rovaya.DAL.Repository.Business;
using System.Security.Claims;
using System.Text.Json;

namespace Rovaya.BLL.Service.Business
{
    public class BusinessPlaceService : IBusinessPlaceService
    {
        private readonly IBusinessPlaceRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IAuditLogService _audit;

        public BusinessPlaceService(
            IBusinessPlaceRepository repo,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IConfiguration config,
            IAuditLogService audit)
        {
            _repo = repo;
            _userManager = userManager;
            _env = env;
            _config = config;
            _audit = audit;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? await _userManager.FindByIdAsync(userId) : null;
        }

        /// <summary>
        /// ✅ البوابة الرئيسية: لا يضيف أماكن إلا من حسابه Verified
        /// </summary>
        private async Task<ApplicationUser> EnsureVerifiedAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) throw new Exception("المستخدم غير موجود");
            if (currentUser.VerificationStatus != UserVerificationStatus.Verified)
                throw new Exception("حسابك غير موثّق بعد. يجب تقديم طلب توثيق والموافقة عليه من الإدارة قبل إضافة الأماكن");
            return currentUser;
        }

        private async Task<List<string>> SaveImagesAsync(List<IFormFile>? files)
        {
            var urls = new List<string>();
            if (files == null || !files.Any()) return urls;

            var folder = Path.Combine(_env.WebRootPath, "uploads", "business");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            foreach (var file in files)
            {
                if (file.Length > 5 * 1024 * 1024)
                    throw new Exception("حجم الصورة يتجاوز 5 ميجابايت");

                var ext = Path.GetExtension(file.FileName);
                var name = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(folder, name);
                using (var stream = new FileStream(path, FileMode.Create))
                    await file.CopyToAsync(stream);
                urls.Add($"/uploads/business/{name}");
            }
            return urls;
        }

        private void DeleteImagesFromDisk(string? imagesJson)
        {
            if (string.IsNullOrEmpty(imagesJson)) return;
            var list = JsonSerializer.Deserialize<List<string>>(imagesJson) ?? new();
            foreach (var url in list)
            {
                if (url.StartsWith("http")) continue;
                var fileName = Path.GetFileName(url);
                var path = Path.Combine(_env.WebRootPath, "uploads", "business", fileName);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private string GetFullImageUrl(string relative)
        {
            if (string.IsNullOrEmpty(relative)) return "";
            if (relative.StartsWith("http")) return relative;
            var appUrl = _config["AppUrl"] ?? "https://rovayabackend.runasp.net";
            if (!appUrl.StartsWith("http")) appUrl = "https://" + appUrl;
            return $"{appUrl.TrimEnd('/')}{relative}";
        }

        public async Task<BusinessPlaceResponseDto> CreateAsync(ClaimsPrincipal user, CreateBusinessPlaceDto dto)
        {
            var currentUser = await EnsureVerifiedAsync(user);

            var newImages = await SaveImagesAsync(dto.Images);

            var place = new BusinessPlace
            {
                OwnerId = currentUser.Id,
                Name = dto.Name,
                Category = dto.Category,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Address = dto.Address,
                Description = dto.Description,
                Phone = dto.Phone,
                WorkingHours = dto.WorkingHours,
                ImagesJson = newImages.Any() ? JsonSerializer.Serialize(newImages) : null,
                IsActive = true
            };

            var created = await _repo.CreateAsync(place);

            await _audit.LogActionAsync(
                user: user,
                action: "CreateBusinessPlace",
                targetUserId: currentUser.Id,
                targetUserName: $"{currentUser.FirstName} {currentUser.LastName}",
                details: $"أضاف مكاناً تجارياً: {dto.Name} ({dto.Category}) عند الإحداثيات ({dto.Latitude}, {dto.Longitude})",
                reason: "إضافة مكان تجاري موثّق"
            );

            return MapToDto(created);
        }

        public async Task<List<BusinessPlaceResponseDto>> GetMyPlacesAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) return new();
            var places = await _repo.GetByOwnerIdAsync(currentUser.Id);
            return places.Select(MapToDto).ToList();
        }

        public async Task<List<BusinessPlaceResponseDto>> GetAllActiveAsync()
        {
            var places = await _repo.GetAllActiveAsync();
            return places.Select(MapToDto).ToList();
        }

        public async Task<BusinessPlaceResponseDto> UpdateAsync(ClaimsPrincipal user, UpdateBusinessPlaceDto dto)
        {
            var currentUser = await EnsureVerifiedAsync(user);
            var place = await _repo.GetByIdAsync(dto.Id);
            if (place == null) throw new Exception("المكان غير موجود");
            if (place.OwnerId != currentUser.Id) throw new Exception("لا تملك صلاحية تعديل هذا المكان");

            var oldImages = string.IsNullOrEmpty(place.ImagesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(place.ImagesJson) ?? new();

            var kept = dto.ExistingImages ?? new();
            var toDelete = oldImages.Where(u => !kept.Contains(u)).ToList();
            foreach (var url in toDelete)
            {
                if (!url.StartsWith("http"))
                {
                    var fileName = Path.GetFileName(url);
                    var path = Path.Combine(_env.WebRootPath, "uploads", "business", fileName);
                    if (File.Exists(path)) File.Delete(path);
                }
            }

            var newImages = await SaveImagesAsync(dto.NewImages);
            var allImages = kept.Concat(newImages).ToList();

            place.Name = dto.Name;
            place.Category = dto.Category;
            place.Latitude = dto.Latitude;
            place.Longitude = dto.Longitude;
            place.Address = dto.Address;
            place.Description = dto.Description;
            place.Phone = dto.Phone;
            place.WorkingHours = dto.WorkingHours;
            place.IsActive = dto.IsActive;
            place.ImagesJson = allImages.Any() ? JsonSerializer.Serialize(allImages) : null;
            place.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(place);

            await _audit.LogActionAsync(
                user: user,
                action: "UpdateBusinessPlace",
                targetUserId: currentUser.Id,
                targetUserName: $"{currentUser.FirstName} {currentUser.LastName}",
                details: $"عدّل بيانات المكان #{dto.Id}: {dto.Name}",
                reason: "تعديل مكان تجاري"
            );

            return MapToDto(place);
        }

        public async Task<bool> DeleteAsync(ClaimsPrincipal user, int id)
        {
            var currentUser = await EnsureVerifiedAsync(user);
            var place = await _repo.GetByIdAsync(id);
            if (place == null) return false;
            if (place.OwnerId != currentUser.Id) throw new Exception("لا تملك صلاحية حذف هذا المكان");

            DeleteImagesFromDisk(place.ImagesJson);
            await _repo.DeleteAsync(id);

            await _audit.LogActionAsync(
                user: user,
                action: "DeleteBusinessPlace",
                targetUserId: currentUser.Id,
                targetUserName: $"{currentUser.FirstName} {currentUser.LastName}",
                details: $"حذف المكان التجاري: {place.Name}",
                reason: "حذف مكان تجاري"
            );

            return true;
        }

        private BusinessPlaceResponseDto MapToDto(BusinessPlace p)
        {
            var images = string.IsNullOrEmpty(p.ImagesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(p.ImagesJson) ?? new();

            return new BusinessPlaceResponseDto
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner != null ? $"{p.Owner.FirstName} {p.Owner.LastName}" : null,
                Name = p.Name,
                Category = p.Category,
                CategoryLabel = p.Category.ToString(),
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Address = p.Address,
                Description = p.Description,
                Phone = p.Phone,
                WorkingHours = p.WorkingHours,
                Images = images.Select(GetFullImageUrl).ToList(),
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }


        // ✅ ✅ ✅ الحذف من قبل الإدارة
        // ✅ ✅ ✅ الحذف من قبل الإدارة (نسخة مصححة وآمنة)
        public async Task<bool> DeleteByAdminAsync(ClaimsPrincipal adminUser, int id)
        {
            var place = await _repo.GetByIdAsync(id);
            if (place == null) return false;

            var admin = await _userManager.FindByIdAsync(
                adminUser.FindFirstValue(ClaimTypes.NameIdentifier));
            if (admin == null) throw new Exception("المدير غير موجود");

            // جلب اسم المالك لتسجيله في الـ Audit
            var owner = await _userManager.FindByIdAsync(place.OwnerId);
            var ownerName = owner != null ? $"{owner.FirstName} {owner.LastName}" : "مستخدم محذوف";

            // ⚠️ ملاحظة: حذف الصور من الـ Storage يُترك للـ Repository/Service الخاص بالصور
            //    إذا كانت الصور في جدول منفصل BusinessPlaceImage، احذفها هنا عبر:
            //    await _imageRepo.DeleteByPlaceIdAsync(id);
            //    وإلا فحذف سجل المكان من قاعدة البيانات يكفي، والصور تصبح معزولة
            //    (يمكن تنظيفها لاحقاً بمهمة دورية).

            // ✅ حذف من قاعدة البيانات
            await _repo.DeleteAsync(id);

            // ✅ تسجيل في الـ Audit
            await _audit.LogActionAsync(
                user: adminUser,
                action: "AdminDeleteBusinessPlace",
                targetUserId: place.OwnerId,
                targetUserName: ownerName,
                details: $"قامت الإدارة بحذف المكان التجاري \"{place.Name}\" (ID: {id}) المملوك للمستخدم {ownerName}",
                reason: "إجراء إداري"
            );

            return true;
        }

        // ✅ ✅ ✅ الإظهار/الإخفاء من قبل الإدارة (نسخة مصححة)
        public async Task<bool> ToggleActiveByAdminAsync(ClaimsPrincipal adminUser, int id, bool isActive)
        {
            var place = await _repo.GetByIdAsync(id);
            if (place == null) return false;

            place.IsActive = isActive;
            place.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(place);

            var admin = await _userManager.FindByIdAsync(
                adminUser.FindFirstValue(ClaimTypes.NameIdentifier));

            await _audit.LogActionAsync(
                user: adminUser,
                action: isActive ? "AdminShowBusinessPlace" : "AdminHideBusinessPlace",
                targetUserId: place.OwnerId,
                targetUserName: admin?.Email ?? "admin",
                details: $"تم {(isActive ? "إظهار" : "إخفاء")} المكان \"{place.Name}\" من الخريطة بواسطة الإدارة",
                reason: "إدارة المحتوى"
            );

            return true;
        }





    }
}