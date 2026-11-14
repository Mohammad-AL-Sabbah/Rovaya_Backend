using System.Text.Json.Serialization;

public class ModifyTripRequest
{
    [JsonPropertyName("current_plan")]
    public object CurrentPlan { get; set; }

    [JsonPropertyName("modification_request")]
    public string ModificationRequest { get; set; } = string.Empty;


    [JsonPropertyName("number_of_people")]
    public int NumberOfPeople { get; set; } = 1;



}