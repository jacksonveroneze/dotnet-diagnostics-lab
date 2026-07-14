using System.Diagnostics.CodeAnalysis;
using CorrelationId.DependencyInjection;

namespace JacksonVeroneze.NET.GRPCServer.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class CorrelationIdExtensions
{
    public static IServiceCollection AddCorrelation(
        this IServiceCollection services)
    {
        services.AddDefaultCorrelationId(options =>
        {
            options.EnforceHeader = false;
            options.AddToLoggingScope = true;
            options.IncludeInResponse = true;
            options.CorrelationIdGenerator =
                () => Guid.NewGuid().ToString();
        });

        return services;
    }
}
