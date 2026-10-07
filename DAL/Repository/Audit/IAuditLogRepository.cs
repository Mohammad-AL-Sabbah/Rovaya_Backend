using Rovaya.DAL.Models;

namespace Rovaya.DAL.Repository.Audit
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> CreateAsync(AuditLog log);
        Task<List<AuditLog>> GetAllAsync(int pageNumber = 1, int pageSize = 50);
        Task<int> GetTotalCountAsync();
    }
}