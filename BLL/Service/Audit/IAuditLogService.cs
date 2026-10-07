using Rovaya.DAL.DTO.Response.Audit;
using System.Security.Claims;

namespace Rovaya.BLL.Service.Audit
{
    public interface IAuditLogService
    {
        Task<AuditLogItem?> LogActionAsync(
            ClaimsPrincipal user,
            string action,
            string? targetUserId = null,
            string? targetUserName = null,
            string? details = null,
            string? reason = null);

        Task<AuditLogResponse> GetAuditLogsAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 50);
    }
}