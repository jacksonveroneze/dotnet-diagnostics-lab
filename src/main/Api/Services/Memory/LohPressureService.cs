using System.Buffers;
using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Memory;

public class LohPressureService : ILohPressureService
{
    // the CLR allocates arrays/objects >= 85,000 bytes directly on the Large
    // Object Heap, bypassing Gen0/Gen1.
    private const int MinObjectSizeBytes = 85_000;
    private const int MaxObjectSizeBytes = 5_242_880;
    private const int MaxObjectCount = 2_000;

    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes,
        SimulateType simulateType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(objectCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectCount, MaxObjectCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(objectSizeBytes, MinObjectSizeBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectSizeBytes, MaxObjectSizeBytes);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        if (simulateType == SimulateType.Success)
        {
            RunWithPooling(objectCount, objectSizeBytes, cancellationToken);
        }
        else
        {
            RunWithFragmentation(objectCount, objectSizeBytes, cancellationToken);
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

    private static void RunWithFragmentation(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken)
    {
        // keeps every other object alive while varying the allocated size, so the
        // LOH slots freed by the discarded objects sit between different-sized
        // survivors and can't be reused by later allocations (the LOH isn't
        // compacted by default) — real LOH fragmentation, not just GC pressure.
        List<byte[]> retained = new(objectCount / 2);

        for (var objectIndex = 0; objectIndex < objectCount; objectIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentSizeBytes = objectIndex % 2 == 0
                ? objectSizeBytes
                : objectSizeBytes * 3 / 4;

            var buffer = new byte[currentSizeBytes];
            buffer[0] = 1;

            if (objectIndex % 2 == 0)
            {
                retained.Add(buffer);
            }
        }
    }

    private static void RunWithPooling(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken)
    {
        for (var objectIndex = 0; objectIndex < objectCount; objectIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var buffer = ArrayPool<byte>.Shared.Rent(objectSizeBytes);

            try
            {
                buffer[0] = 1;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
