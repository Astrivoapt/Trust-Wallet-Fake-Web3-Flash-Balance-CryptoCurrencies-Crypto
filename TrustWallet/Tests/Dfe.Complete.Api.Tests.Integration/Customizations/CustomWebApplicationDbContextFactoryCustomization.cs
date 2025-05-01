using System.Net.Http.Headers;
using System.Security.Claims;
using AutoFixture;
using Dfe.Complete.Api.Client.Extensions;
using Dfe.Complete.Application.Common.Mappers;
using Dfe.Complete.Client;
using Dfe.Complete.Client.Contracts;
using Dfe.Complete.Infrastructure.Database;
using Dfe.Complete.Tests.Common.Seeders;
using DfE.CoreLibs.Testing.Mocks.Authentication;
using DfE.CoreLibs.Testing.Mocks.WebApplicationFactory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.Complete.Api.Tests.Integration.Customizations
{
    public class CustomWebApplicationDbContextFactoryCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<CustomWebApplicationDbContextFactory<Program>>(composer => composer.FromFactory(() =>
            {
                var factory = new CustomWebApplicationDbContextFactory<Program>()
                {
                    SeedData = new Dictionary<Type, Action<DbContext>>
                    {
                        { typeof(CompleteContext), context => CompleteContextSeeder.Seed((CompleteContext)context, fixture) } 
                    },
                    ExternalServicesConfiguration = services =>
                    {
                        services.PostConfigure<AuthenticationOptions>(options =>
                        {
                            options.DefaultAuthenticateScheme = "TestScheme";
                            options.DefaultChallengeScheme = "TestScheme";
                        });

                        services.AddAuthentication("TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, MockJwtBearerHandler>("TestScheme", options => { });

                        services.AddAutoMapper(cfg =>
                        {
                            cfg.AddProfile<AutoMapping>();
                        });
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
                        { "CompleteApiClient:BaseUrl", client.BaseAddress!.ToString() }
                    })
                    .Build();

                var services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(config);
                
                services.AddCompleteApiClient<IProjectsClient, ProjectsClient>(config, client);
                services.AddCompleteApiClient<ICsvExportClient, CsvExportClient>(config, client);
                services.AddCompleteApiClient<IUsersClient, UsersClient>(config, client);
                var serviceProvider = services.BuildServiceProvider();
                
                fixture.Inject(factory);
                fixture.Inject(serviceProvider);
                fixture.Inject(client);
                fixture.Inject(serviceProvider.GetRequiredService<IProjectsClient>());
                fixture.Inject(serviceProvider.GetRequiredService<ICsvExportClient>());
                fixture.Inject(serviceProvider.GetRequiredService<IUsersClient>());
                fixture.Inject(new List<Claim>());

                return factory;
            }));
        }
    }
}
