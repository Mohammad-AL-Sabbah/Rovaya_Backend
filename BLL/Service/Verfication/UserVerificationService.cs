using Microsoft.AspNetCore.Identity;
using Rovaya.BLL.Service.Audit;
using Rovaya.DAL.DTO.Request.Business;
using Rovaya.DAL.DTO.Response.Business;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Business;
using Rovaya.DAL.Repository.Business;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Business
{
    public class UserVerificationService : IUserVerificationService
    {
        private readonly IVerificationRequestRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _audit;

        public UserVerificationService(
            IVerificationRequestRepository repo,
            UserManager<ApplicationUser> userManager,
            IAuditLogService audit)
        {
            _repo = repo;
            _userManager = userManager;
            _audit = audit;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? await _userManager.FindByIdAsync(userId) : null;
        }

        private static string StatusLabel(UserVerificationStatus s) => s switch
        {
            UserVerificationStatus.NotVerified => "غير موثّق",
            UserVerificationStatus.Pending => "قيد المراجعة",
            UserVerificationStatus.Verified => "موثّق",
            UserVerificationStatus.Rejected => "مرفوض",
            _ => "غير معروف"
        };

        public async Task<VerificationRequestDto> SubmitRequestAsync(ClaimsPrincipal user, SubmitVerificationRequestDto dto)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) throw new Exception("المستخدم غير موجود");

            if (currentUser.VerificationStatus == UserVerificationStatus.Verified)
                throw new Exception("حسابك موثّق بالفعل، لا حاجة لتقديم طلب جديد");

            if (currentUser.VerificationStatus == UserVerificationStatus.Pending)
                throw new Exception("لديك طلب قيد المراجعة حالياً، يرجى الانتظار");

            var request = new VerificationRequest
            {
                UserId = currentUser.Id,
                RequestTitle = dto.RequestTitle,
                Description = dto.Description,
                ContactPhone = dto.ContactPhone,
                ContactEmail = dto.ContactEmail,
                BusinessName = dto.BusinessName,
                BusinessAddress = dto.BusinessAddress,
                Status = VerificationRequestStatus.Pending
            };

            var created = await _repo.CreateAsync(request);

            currentUser.VerificationStatus = UserVerificationStatus.Pending;
            currentUser.VerificationNotes = null;
            await _userManager.UpdateAsync(currentUser);

            await _audit.LogActionAsync(
                user: user,
                action: "SubmitVerificationRequest",
                targetUserId: currentUser.Id,
                targetUserName: $"{currentUser.FirstName} {currentUser.LastName}",
                details: $"قدّم طلب توثيق بعنوان: {dto.RequestTitle}",
                reason: "طلب توثيق حساب كصاحب عمل"
            );

            return MapToDto(created, currentUser);
        }

        public async Task<UserVerificationStatusResponse> GetMyStatusAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) throw new Exception("المستخدم غير موجود");

            var pending = await _repo.GetPendingByUserIdAsync(currentUser.Id);

            return new UserVerificationStatusResponse
            {
                UserId = currentUser.Id,
                Status = currentUser.VerificationStatus,
                StatusLabel = StatusLabel(currentUser.VerificationStatus),
                CanAddPlaces = currentUser.VerificationStatus == UserVerificationStatus.Verified,
                Notes = currentUser.VerificationNotes,
                VerifiedAt = currentUser.VerifiedAt,
                ActiveRequest = pending != null ? MapToDto(pending, currentUser) : null
            };
        }

        public async Task<List<VerificationRequestDto>> GetAllPendingForAdminAsync(ClaimsPrincipal adminUser)
        {
            var requests = await _repo.GetAllPendingForAdminAsync();
            return requests.Select(r => MapToDto(r, r.User)).ToList();
        }

        public async Task<bool> ReviewRequestAsync(ClaimsPrincipal adminUser, int requestId, ReviewVerificationRequestDto dto)
        {
            var request = await _repo.GetByIdAsync(requestId);
            if (request == null) return false;
            if (request.Status != VerificationRequestStatus.Pending)
                throw new Exception("هذا الطلب تمت مراجعته مسبقاً");

            var admin = await GetCurrentUserAsync(adminUser);
            if (admin == null) throw new Exception("المدير غير موجود");

            var targetUser = request.User;

            if (dto.Approve)
            {
                request.Status = VerificationRequestStatus.Approved;
                targetUser.VerificationStatus = UserVerificationStatus.Verified;
                targetUser.VerificationNotes = string.IsNullOrWhiteSpace(dto.AdminNotes)
                    ? "تم التوثيق بنجاح، يمكنك الآن إضافة أماكنك على خريطة روفايا"
                    : dto.AdminNotes;
                targetUser.VerifiedAt = DateTime.UtcNow;
                targetUser.VerifiedByAdminId = admin.Id;

                await _audit.LogActionAsync(
                    user: adminUser,
                    action: "ApproveVerificationRequest",
                    targetUserId: targetUser.Id,
                    targetUserName: $"{targetUser.FirstName} {targetUser.LastName}",
                    details: $"تم توثيق حساب المستخدم كصاحب عمل تجاري (طلب #{request.Id})",
                    reason: "مراجعة طلب توثيق"
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.AdminNotes))
                    throw new Exception("يجب ذكر سبب الرفض");

                request.Status = VerificationRequestStatus.Rejected;
                targetUser.VerificationStatus = UserVerificationStatus.Rejected;
                targetUser.VerificationNotes = dto.AdminNotes;

                await _audit.LogActionAsync(
                    user: adminUser,
                    action: "RejectVerificationRequest",
                    targetUserId: targetUser.Id,
                    targetUserName: $"{targetUser.FirstName} {targetUser.LastName}",
                    details: $"تم رفض طلب التوثيق #{request.Id}",
                    reason: dto.AdminNotes
                );
            }

            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByAdminId = admin.Id;
            request.AdminNotes = dto.AdminNotes;

            await _repo.UpdateAsync(request);
            await _userManager.UpdateAsync(targetUser);

            return true;
        }

        // ✅ ✅ ✅ الدالة الجديدة للأرشيف
        public async Task<List<VerificationRequestDto>> GetAllRequestsForAdminAsync(ClaimsPrincipal adminUser, VerificationRequestStatus? status = null)
        {
            var requests = await _repo.GetAllForAdminAsync(status);

            // جلب أسماء المدراء الذين راجعوا الطلبات دفعة واحدة (لتجنب N+1 Query)
            var reviewerIds = requests
                .Where(r => !string.IsNullOrEmpty(r.ReviewedByAdminId))
                .Select(r => r.ReviewedByAdminId!)
                .Distinct()
                .ToList();

            var reviewerNames = new Dictionary<string, string>();
            foreach (var rid in reviewerIds)
            {
                var admin = await _userManager.FindByIdAsync(rid);
                if (admin != null)
                {
                    reviewerNames[rid] = $"{admin.FirstName} {admin.LastName}";
                }
            }

            return requests.Select(r =>
            {
                var dto = MapToDto(r, r.User);
                if (!string.IsNullOrEmpty(r.ReviewedByAdminId) && reviewerNames.ContainsKey(r.ReviewedByAdminId))
                {
                    dto.ReviewedByAdminName = reviewerNames[r.ReviewedByAdminId];
                }
                return dto;
            }).ToList();
        }

        private VerificationRequestDto MapToDto(VerificationRequest r, ApplicationUser user) => new()
        {
            Id = r.Id,
            UserId = user.Id,
            UserName = $"{user.FirstName} {user.LastName}",
            UserEmail = user.Email ?? "",
            RequestTitle = r.RequestTitle,
            Description = r.Description,
            ContactPhone = r.ContactPhone,
            ContactEmail = r.ContactEmail,
            BusinessName = r.BusinessName,
            BusinessAddress = r.BusinessAddress,
            Status = r.Status,
            AdminNotes = r.AdminNotes,
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt,
            ReviewedByAdminName = null // سيتم ملؤها في الدالة أعلاه إذا لزم الأمر
        };
    }
}