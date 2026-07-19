using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Cpu;

public class FibonacciService : IFibonacciService
{
    private const int MinN = 1;
    private const int MaxN = 40;

    public SimulationResult Run(
        int sequencePosition,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(sequencePosition, MinN);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sequencePosition, MaxN);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        FibNaive(sequencePosition, cancellationToken);

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

    // naive recursion, no memoization: O(1.618^n) calls, the classic CPU
    // hot-path shape a profiler like dotTrace surfaces as a single dominant
    // self-time frame.
    private static long FibNaive(int sequencePosition, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return sequencePosition <= 1
            ? sequencePosition
            : FibNaive(sequencePosition - 1, cancellationToken) +
              FibNaive(sequencePosition - 2, cancellationToken);
    }
}
