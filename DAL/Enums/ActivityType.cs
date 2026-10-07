using System.Text.Json.Serialization;

namespace Rovaya.DAL.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivityType
    {
        Family,       // عائلي
        Adventure,    // مغامرات
        Cultural,     // ثقافي
        Religious,    // ديني
        Nature,       // طبيعة
        Shopping,     // تسوق
        Food          // طعام
    }
}