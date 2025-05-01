using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Dfe.Complete.Api.Client.Security
{
    [ExcludeFromCodeCoverage]
    public class BearerTokenHandler(ITokenAcquisitionService tokenAcquisitionService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await tokenAcquisitionService.GetTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
