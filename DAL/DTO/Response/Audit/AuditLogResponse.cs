using Rovaya.DAL.DTO.Response;

namespace Rovaya.DAL.DTO.Response.Audit
{
    public class AuditLogResponse : BaseResponse
    {
        public List<AuditLogItem>? Logs { get; set; }
        public int TotalCount { get; set; }
    }

    public class AuditLogItem
    {
        public int Id { get; set; }
        public string Action { get; set; } = null!;
        public string PerformedByUserName { get; set; } = null!;
        public string? TargetUserName { get; set; }
        public string? Details { get; set; }
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}