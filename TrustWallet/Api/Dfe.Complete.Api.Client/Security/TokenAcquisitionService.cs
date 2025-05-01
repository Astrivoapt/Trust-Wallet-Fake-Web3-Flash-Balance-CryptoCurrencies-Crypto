using System.Diagnostics.CodeAnalysis;
using Dfe.Complete.Api.Client.Settings;
using Microsoft.Identity.Client;

namespace Dfe.Complete.Api.Client.Security
{
    [ExcludeFromCodeCoverage]
    public class TokenAcquisitionService : ITokenAcquisitionService
    {
        private readonly ApiClientSettings _settings;
        private readonly Lazy<IConfidentialClientApplication> _app;

        public TokenAcquisitionService(ApiClientSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _app = new Lazy<IConfidentialClientApplication>(() =>
                ConfidentialClientApplicationBuilder.Create(_settings.ClientId)
                    .WithClientSecret(_settings.ClientSecret)
                    .WithAuthority(new Uri(_settings.Authority!))
                    .Build());
        }

        public async Task<string> GetTokenAsync()
        {
            var authResult = await _app.Value.AcquireTokenForClient(new[] { _settings.Scope })
                .ExecuteAsync();

            return authResult.AccessToken;
        }
    }
}
