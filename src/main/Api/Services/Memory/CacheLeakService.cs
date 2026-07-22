using System.Collections.Concurrent;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class CacheLeakService : ICacheLeakService
{
    private const int MinObjectCount = 1;
    private const int MaxObjectCount = 10_000;
    private const int MinObjectSizeBytes = 1;
    private const int MaxObjectSizeBytes = 1_048_576;

    private static readonly ConcurrentDictionary<Guid, Customer> Cache = new();

    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(objectCount, MinObjectCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectCount, MaxObjectCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(objectSizeBytes, MinObjectSizeBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectSizeBytes, MaxObjectSizeBytes);

        return SimulationRunner.Run(()
            => InternalRun(objectCount, objectSizeBytes));
    }

    private static void InternalRun(
        int objectCount,
        int objectSizeBytes)
    {
        for (var i = 0; i < objectCount; i++)
        {
            var customer = new Customer(Guid.NewGuid(), new byte[objectSizeBytes]);

            Cache[customer.Id] = customer;
        }
    }

    private sealed record Customer(Guid Id, byte[] Data);
}
