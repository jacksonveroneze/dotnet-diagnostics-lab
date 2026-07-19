using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Threads;

public class ThreadPoolStarvationService : IThreadPoolStarvationService
{
    private const int MinDelayMs = 100;
    private const int MaxDelayMs = 10000;
    private const int MinTaskCount = 1;
    private const int MaxTaskCount = 10;

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
        IEnumerable<Task> tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task.Delay(delayMs, cancellationToken).GetAwaiter().GetResult();
            }, cancellationToken));

        await Task.WhenAll(tasks);
    }
}
