using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientApp.Aspire.AppHost.Extensions;

internal static class ResourceBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> AddJWT(this IResourceBuilder<ProjectResource> resource)
    {
        var builder = resource.ApplicationBuilder;
        var jwtSection = builder.Configuration.GetSection("Jwt");
        resource
            .WithEnvironment("Jwt__Issuer", jwtSection["Issuer"] ?? string.Empty)
            .WithEnvironment("Jwt__Audience", jwtSection["Audience"] ?? string.Empty)
            .WithEnvironment("Jwt__Key", jwtSection["Key"] ?? string.Empty)
            .WithEnvironment("Jwt__ExpirationInMinutes", jwtSection["ExpirationInMinutes"] ?? string.Empty);
        return resource;
    }

    public static IResourceBuilder<ProjectResource> AddScalar(this IResourceBuilder<ProjectResource> resource)
    {
        resource.WithUrlForEndpoint("https", url => {
            url.DisplayText = "Scalar HTTPS";
            url.Url = "/scalar";
        });
        
        return resource;
    }
}