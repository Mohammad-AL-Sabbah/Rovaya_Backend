using System.Text.Json.Serialization;

public class PlaceDetailsRequest
{
    [JsonPropertyName("place_name")]
    public string PlaceName { get; set; } = string.Empty;

    [JsonPropertyName("place_name_en")]
    public string? PlaceNameEn { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}