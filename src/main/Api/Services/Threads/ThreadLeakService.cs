using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Threads;

public class ThreadLeakService : IThreadLeakService
{
    // private const int MaxIterations = 1_00_000;
    //private const int MaxStringLength = 99_000;

    public async Task<SimulationResult> RunAsync(
        int delayMs, 
        int taskCount, 
        SimulateType simulateType, 
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(delayMs, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delayMs, 50000);
        ArgumentOutOfRangeException.ThrowIfLessThan(taskCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taskCount, 99);

        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();

        await InternalRunAsync(delayMs, taskCount);

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter,
            Iterations: 0
        );
    }

    private static async Task InternalRunAsync(
        int delayMs,
        int taskCount)
    {
        await Task.Yield();
        for (var i = 0; i < taskCount; i++)
        {
            var thread = new Thread(() => RecurseAndSleep(depth: 500, delayMs), 
                maxStackSize: 0)
            {
                IsBackground = true,
                Name = $"leaked-thread-{Guid.NewGuid():N}"
            };
            
            
            // stack padrão
            //
            // var thread = new Thread(() =>
            // {
            //     var buffer = new byte[512 * 1024]; // 512KB no heap gerenciado
            //     buffer[0] = 1; // força commit da página
            //     Thread.Sleep(delayMs);
            // })
            // {
            //     IsBackground = true, // evita travar o shutdown do processo
            //     Name = $"leaked-thread-{Guid.NewGuid():N}",
            // };
            //
            thread.Start();
        }
    }
    
    static void RecurseAndSleep(int depth, int sleepMs)
    {
        Span<byte> localBuffer = stackalloc byte[512]; // toca páginas do stack
        if (depth > 0)
            RecurseAndSleep(depth - 1, sleepMs);
        else
            Thread.Sleep(sleepMs);
    }
}
