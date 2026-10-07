using System.Text.Json.Serialization;

namespace Rovaya.DAL.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RecommendationSeason
    {
        Spring,    // ربيع
        Summer,    // صيف
        Autumn,    // خريف
        Winter,    // شتاء
        All        // كل المواسم
    }
}