using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Meteo;

using var channel = GrpcChannel.ForAddress("https://localhost:7777/");
var client = new MeteoService.MeteoServiceClient(channel);
var from = DateTime.Now.AddDays(-7).ToUniversalTime();
var reply = client.GetMeteo(new MeteoRequest {From = from.ToTimestamp()});
while (await reply.ResponseStream.MoveNext())
{
    var curr = reply.ResponseStream.Current;
    if (curr.Status == false)
    {
        Console.WriteLine($"Error on server: {curr.ErrorMessage}");
        continue;
    }
    var date = DateTime.Parse(curr.Time);
    Console.WriteLine($"{DateTime.Now:HH:mm.ss} погода на {date:dd.MM.yyyy H:mm} = {curr.Temperature}");
}