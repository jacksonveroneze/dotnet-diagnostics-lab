using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Memory;

public class MemoryLeakStaticService : IMemoryLeakStaticService
{
    private const int MaxObjectCount = 10_000;
    private const int MaxObjectSizeBytes = 1_048_576;

    // static field is a permanent GC root: objects added here are never collected (intentional leak).
    private static readonly List<byte[]> LeakedObjects = [];
    private static readonly Lock LeakedObjectsLock = new();

    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(objectCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectCount, MaxObjectCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(objectSizeBytes, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectSizeBytes, MaxObjectSizeBytes);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        RunLeaking(objectCount, objectSizeBytes);

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter,
            Iterations: objectCount
        );
    }

    private static void RunLeaking(
        int objectCount,
        int objectSizeBytes)
    {
        for (var i = 0; i < objectCount; i++)
        {
            lock (LeakedObjectsLock)
            {
                LeakedObjects.Add(new byte[objectSizeBytes]);
            }
        }
    }
}
