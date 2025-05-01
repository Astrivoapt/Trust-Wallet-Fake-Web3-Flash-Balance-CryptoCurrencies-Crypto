using System.Diagnostics.CodeAnalysis;
using Dfe.Complete.Api.Client.Security;
using Dfe.Complete.Api.Client.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.Complete.Api.Client.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCompleteApiClient<TClientInterface, TClientImplementation>(
            this IServiceCollection services,
            IConfiguration configuration,
            HttpClient? existingHttpClient = null)
            where TClientInterface : class
            where TClientImplementation : class, TClientInterface
        {
            var apiSettings = new ApiClientSettings(); 
            
            configuration.GetSection("CompleteApiClient").Bind(apiSettings);

            services.AddSingleton(apiSettings);
            services.AddSingleton<ITokenAcquisitionService, TokenAcquisitionService>();

            if (existingHttpClient != null)
            {
                services.AddSingleton(existingHttpClient);
                services.AddTransient<TClientInterface, TClientImplementation>(serviceProvider =>
                {
                    return ActivatorUtilities.CreateInstance<TClientImplementation>(
                        serviceProvider, existingHttpClient, apiSettings.BaseUrl!);
                });
            }
            else
            {
                services.AddHttpClient<TClientInterface, TClientImplementation>((httpClient, serviceProvider) =>
                {
                    httpClient.BaseAddress = new Uri(apiSettings.BaseUrl!);

                    return ActivatorUtilities.CreateInstance<TClientImplementation>(
                        serviceProvider, httpClient, apiSettings.BaseUrl!);
                })
                .AddHttpMessageHandler(serviceProvider =>
                {
                    var tokenService = serviceProvider.GetRequiredService<ITokenAcquisitionService>();
                    return new BearerTokenHandler(tokenService);
                });
            }
            
            return services;
        }
    }
}
