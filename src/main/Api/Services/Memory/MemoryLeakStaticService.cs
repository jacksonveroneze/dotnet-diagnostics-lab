using System.Globalization;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class MemoryLeakStaticService : IMemoryLeakStaticService
{
    private const int MinObjectCount = 1;
    private const int MaxObjectCount = 10_000;
    private const int MinObjectSizeBytes = 1;
    private const int MaxObjectSizeBytes = 1_048_576;

    private static readonly List<Customer> LeakedObjects = [];
    private static readonly Lock LeakedObjectsLock = new();

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
            lock (LeakedObjectsLock)
            {
                var customer = new Customer(
                    Guid.NewGuid(),
                    string.Create(CultureInfo.InvariantCulture, $"name_{i}"),
                    new byte[objectSizeBytes]);
                
                LeakedObjects.Add(customer);
            }
        }
    }
}
