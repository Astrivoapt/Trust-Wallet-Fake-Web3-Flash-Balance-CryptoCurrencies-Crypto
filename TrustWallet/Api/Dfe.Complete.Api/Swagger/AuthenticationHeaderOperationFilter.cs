using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dfe.Complete.Api.Swagger
{
    public class AuthenticationHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();

            var securityScheme = new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Description = "Input your Bearer token in this format - Bearer {your token here}",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    securityScheme, new List<string>()
                }
            });
        }
    }
}
