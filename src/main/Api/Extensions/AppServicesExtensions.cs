using System.Diagnostics.CodeAnalysis;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services;
using JacksonVeroneze.NET.GRPCServer.Api.Services;

namespace JacksonVeroneze.NET.GRPCServer.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class AppServicesExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IMemoryG2PromotionService, MemoryG2PromotionService>();
        services.AddScoped<IMemoryLeakStatic, MemoryLeakStatic>();
        services.AddScoped<IMemoryStringAllocation, MemoryStringAllocation>();

        return services;
    }
}
