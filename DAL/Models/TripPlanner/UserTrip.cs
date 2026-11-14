public class UserTrip
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TripTitle { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string ItineraryJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSaved { get; set; } = true;

    // ✅ أضف هذا الحقل الجديد
    public TripStatus Status { get; set; } = TripStatus.Draft;
}

// ✅ أضف Enum جديد
public enum TripStatus
{
    Draft = 0,      // مسودة (خطة سابقة)
    Active = 1,     // نشطة (الخطة التي يمشي عليها المستخدم)
    Archived = 2    // مؤرشفة (محفوظة لكن غير نشطة)
}