using System.Diagnostics.CodeAnalysis;
using Dfe.Complete.Api.Middleware;
using Dfe.Complete.Api.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FeatureManagement;
using NetEscapades.AspNetCore.SecurityHeaders;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Dfe.Complete.Application.Common.Mappers;
using Dfe.Complete.Infrastructure;
using Dfe.Complete.Infrastructure.Security.Authorization;
using DfE.CoreLibs.Http.Middlewares.CorrelationId;
using DfE.CoreLibs.Http.Interfaces;

namespace Dfe.Complete.Api
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(),
                        TelemetryConverter.Traces)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });

            builder.Services.AddControllers()
                .AddJsonOptions(c => { c.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

            builder.Services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
                
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddSwaggerGen(c =>
            {
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
                c.EnableAnnotations();
            });

            builder.Services.AddCustomAuthorization(builder.Configuration);

            builder.Services.ConfigureOptions<SwaggerOptions>();
            builder.Services.AddFeatureManagement();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();

            builder.Services.AddApplicationDependencyGroup(builder.Configuration);
            builder.Services.AddInfrastructureDependencyGroup(builder.Configuration);

            builder.Services.AddAutoMapper(typeof(AutoMapping));

            builder.Services.AddOptions<SwaggerUIOptions>()
                .Configure<IHttpContextAccessor>((swaggerUiOptions, httpContextAccessor) =>
                {
                    var originalIndexStreamFactory = swaggerUiOptions.IndexStream;
                    swaggerUiOptions.IndexStream = () =>
                    {
                        using var originalStream = originalIndexStreamFactory();
                        using var originalStreamReader = new StreamReader(originalStream);
                        var originalIndexHtmlContents = originalStreamReader.ReadToEnd();
                        var requestSpecificNonce = httpContextAccessor?.HttpContext?.GetNonce();
                        var nonceEnabledIndexHtmlContents = originalIndexHtmlContents
                            .Replace("<script", $"<script nonce=\"{requestSpecificNonce}\" ",
                                StringComparison.OrdinalIgnoreCase)
                            .Replace("<style", $"<style nonce=\"{requestSpecificNonce}\" ",
                                StringComparison.OrdinalIgnoreCase);
                        return new MemoryStream(Encoding.UTF8.GetBytes(nonceEnabledIndexHtmlContents));
                    };
                });

            var appInsightsCnnStr = builder.Configuration.GetSection("ApplicationInsights")?["ConnectionString"];
            if (!string.IsNullOrWhiteSpace(appInsightsCnnStr))
            {
                builder.Services.AddApplicationInsightsTelemetry(opt => { opt.ConnectionString = appInsightsCnnStr; });
            }

            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            builder.Services.AddOpenApiDocument(configure =>
            {
                configure.Title = "Api";
            });

            var app = builder.Build();

            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false
            };
            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardOptions);

            app.UseSecurityHeaders(options =>
            {
                options.AddFrameOptionsDeny()
                    .AddXssProtectionDisabled()
                    .AddContentTypeOptionsNoSniff()
                    .RemoveServerHeader()
                    .AddContentSecurityPolicy(builder =>
                    {
                        builder.AddDefaultSrc().Self();
                        builder.AddStyleSrc().Self().WithNonce();
                        builder.AddScriptSrc().Self().WithNonce();
                    })
                    .AddPermissionsPolicy(builder =>
                    {
                        builder.AddAccelerometer().None();
                        builder.AddAutoplay().None();
                        builder.AddCamera().None();
                        builder.AddEncryptedMedia().None();
                        builder.AddFullscreen().None();
                        builder.AddGeolocation().None();
                        builder.AddGyroscope().None();
                        builder.AddMagnetometer().None();
                        builder.AddMicrophone().None();
                        builder.AddMidi().None();
                        builder.AddPayment().None();
                        builder.AddPictureInPicture().None();
                        builder.AddSyncXHR().None();
                        builder.AddUsb().None();
                    });
            });

            app.UseHsts();
            app.UseHttpsRedirection();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
                }

                c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete);
            });

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseMiddleware<UrlDecoderMiddleware>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Logger is working...");

            await app.RunAsync();
        }
    }
}
