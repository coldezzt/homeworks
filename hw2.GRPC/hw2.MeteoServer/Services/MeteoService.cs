using Grpc.Core;
using Meteo;
using MeteoServer.Models;

namespace MeteoServer.Services;

public class MeteoService : Meteo.MeteoService.MeteoServiceBase
{
    private const string ApiUrl = "https://api.open-meteo.com/v1/forecast?latitude=55.792139&longitude=49.122135&hourly=temperature_2m&start_hour={START_HOUR}&end_hour={END_HOUR}";

    public override async Task GetMeteo(MeteoRequest request, IServerStreamWriter<MeteoResponse> responseStream, ServerCallContext context)
    {
        var httpClient = new HttpClient();
        var currentTime = request.From.ToDateTime();
        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                var url = ApiUrl
                    .Replace("{START_HOUR}", currentTime.ToString("s"))
                    .Replace("{END_HOUR}", currentTime.ToString("s"));
                var response = await httpClient.GetAsync(url);
                var apiResponse = await response.Content.ReadFromJsonAsync<MeteoApiResponse>();
                if (apiResponse is null) throw new NullReferenceException("No data from open-meteo returned.");

                var resp = new MeteoResponse
                {
                    Time = apiResponse.HourlyData.Time[0],
                    Temperature = apiResponse.HourlyData.Temperature2m[0],
                    Status = true,
                    ErrorMessage = ""
                };
                await responseStream.WriteAsync(resp);
            }
            catch (Exception e)
            {
                var resp = new MeteoResponse
                {
                    Status = false,
                    ErrorMessage = e.Message
                };
                await responseStream.WriteAsync(resp);
            }
            finally
            {
                await Task.Delay(1000);
                currentTime = currentTime.AddHours(2);
            }
        }
    }
}