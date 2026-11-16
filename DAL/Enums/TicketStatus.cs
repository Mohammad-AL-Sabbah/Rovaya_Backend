namespace Rovaya.DAL.Enums
{
    public enum TicketStatus
    {
        Sent = 0,          // أرسلت من المستخدم
        Received = 1,      // استلمها الدعم الفني
        InProgress = 2,    // قيد المعالجة
        Resolved = 3,      // تم الحل
        Rejected = 4       // مرفوضة (خطأ من المستخدم أو لا تنطبق)
    }
}