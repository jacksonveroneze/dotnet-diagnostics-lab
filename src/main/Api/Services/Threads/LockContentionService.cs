using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Threads;

public class LockContentionService : ILockContentionService
{
    private const int MinDelayMs = 100;
    private const int MaxDelayMs = 10000;
    private const int MinTaskCount = 1;
    private const int MaxTaskCount = 10;

    private static readonly Lock SharedLock = new();

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
            => InternalRunAsync(delayMs, taskCount, cancellationToken));
    }

    private static async Task InternalRunAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken)
    {
        long sharedCounter = 0;

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
}
