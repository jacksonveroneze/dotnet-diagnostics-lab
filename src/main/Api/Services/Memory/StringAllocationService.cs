using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Memory;

public class StringAllocationService : IStringAllocationService
{
    private const int MaxIterations = 1_00_000;
    private const int MaxStringLength = 99_000;

    public SimulationResult Run(
        int iterations,
        int stringLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, MaxIterations);
        ArgumentOutOfRangeException.ThrowIfLessThan(stringLength, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stringLength, MaxStringLength);

        var gcBefore = GcMetrics.CollectionCount();
        var chunk = new string('_', stringLength);

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        RunWithConcatenation(iterations, chunk);

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter,
            Iterations: iterations
        );
    }

    private static void RunWithConcatenation(
        int iterations,
        string chunk)
    {
        var acc = string.Empty;

        for (var i = 0; i < iterations; i++)
        {
            acc += chunk;
        }
    }
}
