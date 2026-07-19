using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class CultureExtensions
{
    private const string DefaultCulture = "pt-BR";

    public static IServiceCollection AddCultureConfiguration(
        this IServiceCollection services)
    {
        CultureInfo defaultCulture = new(DefaultCulture);

        CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

        CultureInfo[] supportedCultures = [new(DefaultCulture)];

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        return services;
    }
}
