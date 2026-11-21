namespace Rovaya.DAL.Enums
{
    /// <summary>
    /// حالة توثيق حساب المستخدم كصاحب عمل تجاري
    /// </summary>
    public enum UserVerificationStatus
    {
        NotVerified = 0,  // غير موثق (الحالة الافتراضية لكل مستخدم جديد)
        Pending = 1,      // طلب التحقق قيد المراجعة
        Verified = 2,     // موثق - يمكنه إضافة أماكن
        Rejected = 3      // مرفوض - يمكنه إعادة التقديم بعد فترة
    }
}