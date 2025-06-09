using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Secure;

namespace hw2.SecureServer.Services;

[Authorize]
public class SecureService : Secure.SecureService.SecureServiceBase
{
    public override Task<SecretResponse> GetSecret(Empty request, ServerCallContext context) => 
        Task.FromResult(new SecretResponse {Message = "The Cake is a lie."});
}