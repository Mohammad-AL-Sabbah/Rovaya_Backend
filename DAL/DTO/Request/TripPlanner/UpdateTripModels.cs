using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// طلب تعديل رحلة محفوظة. التعديل يتم بعمليات محددة (حذف / إضافة) وليس بإرسال الخطة كاملة،
/// حتى يبقى السيرفر هو المتحكم ببنية الخطة ولا تنكسر.
/// </summary>
public class UpdateTripRequest
{
    [JsonPropertyName("operations")]
    public List<TripOperation> Operations { get; set; } = new();

    /// <summary>اختياري: عدد الأشخاص لإعادة حساب التكاليف. إن لم يُرسل يُستنتج من الخطة المخزنة.</summary>
    [JsonPropertyName("number_of_people")]
    public int? NumberOfPeople { get; set; }
}

public class TripOperation
{
    /// <summary>remove_activity | add_activity</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("day_number")]
    public int DayNumber { get; set; }

    /// <summary>لـ remove_activity: رقم النشاط داخل اليوم (يبدأ من 0)</summary>
    [JsonPropertyName("activity_index")]
    public int? ActivityIndex { get; set; }

    /// <summary>لـ add_activity</summary>
    [JsonPropertyName("activity")]
    public NewActivityDto? Activity { get; set; }
}

public class NewActivityDto
{
    [JsonPropertyName("place_name")]
    public string PlaceName { get; set; } = string.Empty;

    [JsonPropertyName("place_name_en")]
    public string? PlaceNameEn { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>visit | food | activity (الافتراضي visit)</summary>
    [JsonPropertyName("activity_type")]
    public string? ActivityType { get; set; }

    [JsonPropertyName("cost_per_person")]
    public decimal? CostPerPerson { get; set; }

    /// <summary>اختياري بصيغة HH:mm. إن لم يُرسل يُوضع بعد آخر نشاط في اليوم بـ 30 دقيقة.</summary>
    [JsonPropertyName("start_time")]
    public string? StartTime { get; set; }

    /// <summary>اختياري: مدة الزيارة بالدقائق (15 - 480). الافتراضي 60.</summary>
    [JsonPropertyName("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("description_en")]
    public string? DescriptionEn { get; set; }

    [JsonPropertyName("tips")]
    public string? Tips { get; set; }

    [JsonPropertyName("tips_en")]
    public string? TipsEn { get; set; }
}

public class UpdateTripResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
    public JsonElement? Plan { get; set; }

    public static UpdateTripResult Fail(int statusCode, string message) =>
        new() { Success = false, StatusCode = statusCode, Message = message };

    public static UpdateTripResult Ok(string message, JsonElement plan) =>
        new() { Success = true, StatusCode = 200, Message = message, Plan = plan };
}