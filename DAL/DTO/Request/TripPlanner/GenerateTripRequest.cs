using System.Text.Json.Serialization;

public class GenerateTripRequest
{
    [JsonPropertyName("cities")]
    public List<string> Cities { get; set; } = new();

    [JsonPropertyName("number_of_days")]
    public int NumberOfDays { get; set; }

    [JsonPropertyName("number_of_people")]
    public int NumberOfPeople { get; set; }

    [JsonPropertyName("budget")]
    public decimal Budget { get; set; }
    
    [JsonPropertyName("interests")]
    public List<string> Interests { get; set; } = new();

    [JsonPropertyName("start_date")]
    public string StartDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

    [JsonPropertyName("activity_level")]
    public string ActivityLevel { get; set; } = "moderate";


    [JsonPropertyName("previous_plan")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? PreviousPlan { get; set; }


}