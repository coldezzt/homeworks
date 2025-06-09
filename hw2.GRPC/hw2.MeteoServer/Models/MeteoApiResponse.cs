using System.Text.Json.Serialization;

namespace MeteoServer.Models;

public class MeteoApiResponse
{
    [JsonPropertyName("hourly")]
    public HourlyData HourlyData { get; set; }
}