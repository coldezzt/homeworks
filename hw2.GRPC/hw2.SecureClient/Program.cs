

using System.Net.Http.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Secure;

var c = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7219/"),
    DefaultRequestVersion = new Version(2, 0)
};

var get = await c.GetAsync("");
Console.WriteLine($"Answer on 'GET': {await get.Content.ReadAsStringAsync()}");
var authData = new {Name = "GLaDOS", Password = "Science!"};
var resp = await c.PostAsync("", JsonContent.Create(authData));
var token = await resp.Content.ReadAsStringAsync();
Console.WriteLine($"Answer on 'POST': {token}");

var channel = GrpcChannel.ForAddress("https://localhost:7219/");
var client = new SecureService.SecureServiceClient(channel);
var meta = new Metadata {{"Authorization", "Bearer " + token}};
var secret = await client.GetSecretAsync(new Empty(), headers: meta);
Console.WriteLine($"Answer through 'GRPC' with metadata: {secret}");
try
{
    await client.GetSecretAsync(new Empty());
}
catch (Exception ex)
{
    Console.WriteLine($"Answer through 'GRPC' without metadata: {ex.Message}");
}
