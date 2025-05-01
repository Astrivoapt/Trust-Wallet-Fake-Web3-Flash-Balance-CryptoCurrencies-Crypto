using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dfe.Complete.Api.Swagger
{
    public class SwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        private const string ServiceTitle = "API";
        private const string ContactName = "Support";
        private const string ContactEmail = "update_to_contact_email_here";
        private const string DepreciatedMessage = "- API version has been depreciated.";
        
        public SwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;
        
        public void Configure(string? name, SwaggerGenOptions options) => Configure(options);
        
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var desc in _provider.ApiVersionDescriptions)
            {
                var openApiInfo = new OpenApiInfo
                {
                    Title = ServiceTitle,
                    Contact = new OpenApiContact
                    {
                        Name = ContactName,
                        Email = ContactEmail 
                    },
                    Version = desc.ApiVersion.ToString()
                };
                if (desc.IsDeprecated) openApiInfo.Description += DepreciatedMessage;
                
                options.SwaggerDoc(desc.GroupName, openApiInfo);
            }

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            options.MapType<FileResult>(() =>
            {
                return new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                };
            });

            options.OperationFilter<AuthenticationHeaderOperationFilter>();
        }
    }
}
