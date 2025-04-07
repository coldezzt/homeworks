using System.Text.Json.Serialization;

namespace MeteoServer.Models;

public class HourlyData
{
    [JsonPropertyName("time")]
    public string[] Time { get; set; }
    
    [JsonPropertyName("temperature_2m")]
    public double[] Temperature2m { get; set; }
}