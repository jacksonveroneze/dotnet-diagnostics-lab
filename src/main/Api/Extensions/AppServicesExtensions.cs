using System.Diagnostics.CodeAnalysis;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Exception;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Cpu;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Exception;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Threads;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class AppServicesExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IMemoryLeakStaticService, MemoryLeakStaticService>();
        services.AddScoped<IStringAllocationService, StringAllocationService>();
        services.AddScoped<ILohPressureService, LohPressureService>();
        services.AddSingleton<IEventLeakService, EventLeakService>();
        services.AddScoped<ICacheLeakService, CacheLeakService>();
        services.AddSingleton<IClosureLeakService, ClosureLeakService>();
        services.AddScoped<ICancellationTokenSourceLeakService, CancellationTokenSourceLeakService>();
        services.AddSingleton<ITimerLeakService, TimerLeakService>();

        services.AddScoped<IThreadPoolStarvationService, ThreadPoolStarvationService>();
        services.AddScoped<IThreadLeakService, ThreadLeakService>();
        services.AddScoped<ILockContentionService, LockContentionService>();

        services.AddScoped<IFibonacciService, FibonacciService>();
        services.AddScoped<IRegexBacktrackingService, RegexBacktrackingService>();

        services.AddScoped<IExceptionSimulationService, ExceptionSimulationService>();

        return services;
    }
}
