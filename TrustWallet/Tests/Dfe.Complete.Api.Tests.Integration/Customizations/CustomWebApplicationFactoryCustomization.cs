using System.Net.Http.Headers;
using System.Security.Claims;
using AutoFixture;
using DfE.CoreLibs.Testing.Mocks.Authentication;
using DfE.CoreLibs.Testing.Mocks.WebApplicationFactory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.Complete.Api.Tests.Integration.Customizations
{
    public class CustomWebApplicationFactoryCustomization<TProgram> : ICustomization
        where TProgram : class {

        public void Customize(IFixture fixture)
        {
            fixture.Customize<CustomWebApplicationFactory<TProgram>>(composer => composer.FromFactory(() =>
            {
                var factory = new CustomWebApplicationFactory<TProgram>()
                {
                    ExternalServicesConfiguration = services =>
                    {
                        services.PostConfigure<AuthenticationOptions>(options =>
                        {
                            options.DefaultAuthenticateScheme = "TestScheme";
                            options.DefaultChallengeScheme = "TestScheme";
                        });

                        services.AddAuthentication("TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, MockJwtBearerHandler>("TestScheme", _ => { });
                    },
                    ExternalHttpClientConfiguration = client =>
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "external-mock-token");
                    }
                };

                var client = factory.CreateClient();

                var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "ApiClient:BaseUrl", client.BaseAddress!.ToString() }
                    })
                    .Build();


                var services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(config);

                var serviceProvider = services.BuildServiceProvider();

                fixture.Inject(factory);
                fixture.Inject(serviceProvider);
                fixture.Inject(client);
                fixture.Inject(new List<Claim>());

                return factory;
            }));

        }
    }
}
