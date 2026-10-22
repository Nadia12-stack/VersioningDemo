using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VersioningDemo.Api.Swagger;

/// <summary>Creates one Swagger document per API version.</summary>
public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Versioning Demo API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated
                    ? "This API version is DEPRECATED. Please migrate to the latest version."
                    : "Current API version."
            });
        }
    }
}
