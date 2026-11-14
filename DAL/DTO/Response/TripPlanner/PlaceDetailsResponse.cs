using System.Text.Json.Serialization;

public class PlaceDetailsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;

    [JsonPropertyName("details_ar")]
    public string DetailsAr { get; set; } = string.Empty;

    [JsonPropertyName("details_en")]
    public string DetailsEn { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = new();

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("image_count")]
    public int ImageCount { get; set; }

    [JsonPropertyName("place_name_ar")]
    public string PlaceNameAr { get; set; } = string.Empty;

    [JsonPropertyName("place_name_en")]
    public string PlaceNameEn { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("is_from_dataset")]
    public bool IsFromDataset { get; set; }
}