using System.Diagnostics.CodeAnalysis;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Services;
using JacksonVeroneze.NET.GRPCServer.Api.Services.Cpu;
using JacksonVeroneze.NET.GRPCServer.Api.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Services.Threads;

namespace JacksonVeroneze.NET.GRPCServer.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class AppServicesExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IGen2PromotionService, Gen2PromotionService>();
        services.AddSingleton<IMemoryLeakStaticService, MemoryLeakStaticService>();
        services.AddScoped<IStringAllocationService, StringAllocationService>();
        services.AddScoped<ILohPressureService, LohPressureService>();

        services.AddScoped<IThreadPoolStarvationService, ThreadPoolStarvationService>();
        services.AddScoped<IThreadLeakService, ThreadLeakService>();
        services.AddScoped<ILockContentionService, LockContentionService>();

        services.AddScoped<IFibonacciService, FibonacciService>();

        return services;
    }
}
