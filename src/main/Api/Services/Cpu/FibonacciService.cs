using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Cpu;

public class FibonacciService : IFibonacciService
{
    private const int MinN = 1;
    private const int MaxN = 40;

    public SimulationResult Run(
        int n,
        SimulateType simulateType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(n, MinN);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, MaxN);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        if (simulateType == SimulateType.Success)
        {
            FibIterative(n, cancellationToken);
        }
        else
        {
            FibNaive(n, cancellationToken);
        }

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter,
            Iterations: n
        );
    }

    // naive recursion, no memoization: O(1.618^n) calls, the classic CPU
    // hot-path shape a profiler like dotTrace surfaces as a single dominant
    // self-time frame.
    private static long FibNaive(int n, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return n <= 1
            ? n
            : FibNaive(n - 1, cancellationToken) + FibNaive(n - 2, cancellationToken);
    }

    private static long FibIterative(int n, CancellationToken cancellationToken)
    {
        long a = 0, b = 1;

        for (var i = 0; i < n; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            (a, b) = (b, a + b);
        }

        return a;
    }
}
