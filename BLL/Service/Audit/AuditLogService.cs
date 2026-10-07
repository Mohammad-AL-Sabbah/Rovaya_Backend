using Microsoft.AspNetCore.Identity;
using Rovaya.DAL.DTO.Response;
using Rovaya.DAL.DTO.Response.Audit;
using Rovaya.DAL.Models;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Repository.Audit;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Audit
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogService(IAuditLogRepository auditLogRepository, UserManager<ApplicationUser> userManager)
        {
            _auditLogRepository = auditLogRepository;
            _userManager = userManager;
        }

        public async Task<AuditLogItem?> LogActionAsync(
            ClaimsPrincipal user,
            string action,
            string? targetUserId = null,
            string? targetUserName = null,
            string? details = null,
            string? reason = null)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return null;

                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null) return null;

                var log = new AuditLog
                {
                    Action = action,
                    PerformedByUserId = userId,
                    PerformedByUserName = $"{currentUser.FirstName} {currentUser.LastName}",
                    TargetUserId = targetUserId,
                    TargetUserName = targetUserName,
                    Details = details,
                    Reason = reason,
                    Timestamp = DateTime.UtcNow
                };

                var createdLog = await _auditLogRepository.CreateAsync(log);

                return new AuditLogItem
                {
                    Id = createdLog.Id,
                    Action = createdLog.Action,
                    PerformedByUserName = createdLog.PerformedByUserName,
                    TargetUserName = createdLog.TargetUserName,
                    Details = createdLog.Details,
                    Reason = createdLog.Reason,
                    Timestamp = createdLog.Timestamp
                };
            }
            catch
            {
                // لا نوقف العملية إذا فشل تسجيل الـ Audit
                return null;
            }
        }

        public async Task<AuditLogResponse> GetAuditLogsAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUser = await _userManager.FindByIdAsync(userId);
                if (currentUser == null)
                {
                    return new AuditLogResponse { Success = false, Message = "غير مصرح" };
                }

                var roles = await _userManager.GetRolesAsync(currentUser);
                if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
                {
                    return new AuditLogResponse { Success = false, Message = "لا تملك صلاحية عرض السجلات" };
                }

                var logs = await _auditLogRepository.GetAllAsync(pageNumber, pageSize);
                var totalCount = await _auditLogRepository.GetTotalCountAsync();

                return new AuditLogResponse
                {
                    Success = true,
                    Message = $"تم جلب {logs.Count} سجل",
                    Logs = logs.Select(l => new AuditLogItem
                    {
                        Id = l.Id,
                        Action = l.Action,
                        PerformedByUserName = l.PerformedByUserName,
                        TargetUserName = l.TargetUserName,
                        Details = l.Details,
                        Reason = l.Reason,
                        Timestamp = l.Timestamp
                    }).ToList(),
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                return new AuditLogResponse { Success = false, Message = $"حدث خطأ: {ex.Message}" };
            }
        }
    }
}