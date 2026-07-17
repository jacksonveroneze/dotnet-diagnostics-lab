using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Threads;

public class ThreadPoolStarvationService : IThreadPoolStarvationService
{
    private const int MaxIterations = 1_00_000;
    //private const int MaxStringLength = 99_000;

    public async Task<SimulationResult> RunAsync(
        int iterations,
        int delayMs,
        int taskCount,
        SimulateType simulateType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, MaxIterations);
        ArgumentOutOfRangeException.ThrowIfLessThan(delayMs, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delayMs, 10000);
        ArgumentOutOfRangeException.ThrowIfLessThan(taskCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taskCount, 10);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        await InternalRunAsync(delayMs);

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

    private static async Task InternalRunAsync(
        int delayMs)
    {
        await Task.Yield();
        Thread.Sleep(delayMs);
        //await Task.Delay(delayMs);
        
        // var tasks = Enumerable.Range(0, taskCount)
        //     .Select(_ => Task.Run(async () =>
        //     {
        //         await Task.Delay(delayMs);
        //     }));
        //
        // await Task.WhenAll(tasks);
    }
}
