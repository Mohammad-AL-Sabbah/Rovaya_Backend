using System.Text.Json.Serialization;

namespace Rovaya.DAL.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BlockDurationUnit
    {
        Minutes,
        Hours,
        Days,
        Months,
        Years
    }
}