using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Threads;

public class ThreadPoolStarvationService : IThreadPoolStarvationService
{
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
            await RunNonStarvingAsync(delayMs, taskCount, cancellationToken);
        }
        else
        {
            await RunStarvingAsync(delayMs, taskCount, cancellationToken);
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

    private static async Task RunStarvingAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken)
    {
        // intentional pool blocking: lab exception to the repo's general async
        // rules, to reproduce real thread pool starvation under concurrent load.
        IEnumerable<Task> tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(delayMs);
            }, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private static async Task RunNonStarvingAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken)
    {
        IEnumerable<Task> tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Delay(delayMs, cancellationToken));

        await Task.WhenAll(tasks);
    }
}
