using System.Diagnostics.CodeAnalysis;
using Asp.Versioning;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class AppVersioningExtensions
{
    public static IServiceCollection AddAppVersioning(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader());
        });

        return services;
    }
}
