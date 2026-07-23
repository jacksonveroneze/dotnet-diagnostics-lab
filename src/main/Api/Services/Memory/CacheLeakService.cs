using System.Globalization;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class CacheLeakService(HybridCache cache) : ICacheLeakService
{
    private const int MinObjectCount = 1;
    private const int MaxObjectCount = 10_000;
    private const int MinObjectSizeBytes = 1;
    private const int MaxObjectSizeBytes = 1_048_576;

    private static readonly HybridCacheEntryOptions NeverExpireEntryOptions = new()
    {
        Expiration = TimeSpan.FromDays(3650),
        LocalCacheExpiration = TimeSpan.FromDays(3650),
    };

    public async Task<SimulationResult> RunAsync(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(objectCount, MinObjectCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectCount, MaxObjectCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(objectSizeBytes, MinObjectSizeBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectSizeBytes, MaxObjectSizeBytes);

        return await SimulationRunner.RunAsync(()
            => InternalRunAsync(objectCount, objectSizeBytes, cancellationToken));
    }

    private async Task InternalRunAsync(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < objectCount; i++)
        {
            var customer = new Customer(
                Guid.NewGuid(),
                string.Create(CultureInfo.InvariantCulture, $"name_{i}"),
                new byte[objectSizeBytes]);

            await cache.SetAsync(
                customer.Id.ToString(), customer, NeverExpireEntryOptions,
                cancellationToken: cancellationToken);
        }
    }
}
