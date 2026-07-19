using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Cpu;

public class FibonacciService : IFibonacciService
{
    private const int MinValue = 1;
    private const int MaxValue = 40;

    public SimulationResult Run(
        int sequencePosition)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sequencePosition, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sequencePosition, MaxValue);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        RunFibonacci(sequencePosition);

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter,
            Iterations: sequencePosition
        );
    }

    private static long RunFibonacci(int sequencePosition)
    {
        return sequencePosition <= 1
            ? sequencePosition
            : RunFibonacci(sequencePosition - 1) +
              RunFibonacci(sequencePosition - 2);
    }
}
