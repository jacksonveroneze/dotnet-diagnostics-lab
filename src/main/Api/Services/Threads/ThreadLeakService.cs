using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Threads;

public class ThreadLeakService : IThreadLeakService
{
    private const int MinDelayMs = 100;
    private const int MaxDelayMs = 50000;
    private const int MinTaskCount = 1;
    private const int MaxTaskCount = 99;

    public async Task<SimulationResult> RunAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfLessThan(delayMs, MinDelayMs);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delayMs, MaxDelayMs);
        ArgumentOutOfRangeException.ThrowIfLessThan(taskCount, MinTaskCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taskCount, MaxTaskCount);

        return await SimulationRunner.RunAsync(()
            => InternalRunAsync(delayMs, taskCount));
    }

    private static async Task InternalRunAsync(
        int delayMs,
        int taskCount)
    {
        await Task.Yield();
        for (var i = 0; i < taskCount; i++)
        {
            var thread = new Thread(() =>
                    RecurseAndSleep(depth: 500, delayMs),
                maxStackSize: 0)
            {
                IsBackground = true,
                Name = $"leaked-thread-{Guid.NewGuid():N}"
            };

            thread.Start();
        }
    }

    static void RecurseAndSleep(int depth, int sleepMs)
    {
        Span<byte> localBuffer = stackalloc byte[512];
        if (depth > 0)
            RecurseAndSleep(depth - 1, sleepMs);
        else
            Thread.Sleep(sleepMs);
    }
}
