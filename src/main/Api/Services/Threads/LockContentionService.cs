using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Threads;

public class LockContentionService : ILockContentionService
{
    // shared across every concurrent request, not just within one call: the
    // point is to reproduce a global application lock (e.g. a badly designed
    // singleton/cache), so contention compounds as concurrent requests pile up.
    private static readonly Lock SharedLock = new();

    public async Task<SimulationResult> RunAsync(
        int delayMs,
        int taskCount,
        SimulateType simulateType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(delayMs, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delayMs, 10000);
        ArgumentOutOfRangeException.ThrowIfLessThan(taskCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taskCount, 10);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        if (simulateType == SimulateType.Success)
        {
            await RunWithoutContentionAsync(delayMs, taskCount, cancellationToken);
        }
        else
        {
            await RunWithContentionAsync(delayMs, taskCount, cancellationToken);
        }

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter,
            Iterations: taskCount
        );
    }

    private static async Task RunWithContentionAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken)
    {
        long sharedCounter = 0;

        // critical section too wide: the "work" runs entirely inside the shared
        // lock, serializing tasks that only needed to contend over the counter
        // increment.
        IEnumerable<Task> tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                lock (SharedLock)
                {
                    Thread.Sleep(delayMs);
                    sharedCounter++;
                }
            }, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private static async Task RunWithoutContentionAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken)
    {
        long sharedCounter = 0;

        IEnumerable<Task> tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Thread.Sleep(delayMs);

                lock (SharedLock)
                {
                    sharedCounter++;
                }
            }, cancellationToken));

        await Task.WhenAll(tasks);
    }
}
