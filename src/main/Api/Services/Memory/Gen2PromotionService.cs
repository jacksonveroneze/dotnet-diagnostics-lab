using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Memory;

public class Gen2PromotionService : IGen2PromotionService
{
    private const int MaxObjectCount = 10_000;
    private const int MaxObjectSizeBytes = 1_048_576;

    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes,
        SimulateType simulateType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(objectCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectCount, MaxObjectCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(objectSizeBytes, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectSizeBytes, MaxObjectSizeBytes);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        if (simulateType == SimulateType.Success)
        {
            RunWithoutPromotion(objectCount, objectSizeBytes, cancellationToken);
        }
        else
        {
            RunWithPromotion(objectCount, objectSizeBytes, cancellationToken);
        }

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

    private static void RunWithPromotion(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken)
    {
        // kept reachable for the whole loop: survives the intermediate Gen0/Gen1
        // collections triggered by the allocations below, so the GC promotes it
        // to Gen2 (mid-life crisis) even though it becomes garbage right after.
        List<byte[]> survivors = new(objectCount);

        for (var i = 0; i < objectCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            survivors.Add(new byte[objectSizeBytes]);
        }
    }

    private static void RunWithoutPromotion(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < objectCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buffer = new byte[objectSizeBytes];
            buffer[0] = 1;
        }
    }
}
