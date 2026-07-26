using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class CancellationTokenSourceLeakService : ICancellationTokenSourceLeakService
{
    private const int MinTaskCount = 1;
    private const int MaxTaskCount = 10_000;
    private const int MinDelayMs = 1;
    private const int MaxDelayMs = 60_000;

    public async Task<SimulationResult> RunAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken)
    {
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
        var tasks = new List<Task>(taskCount);

        for (var i = 0; i < taskCount; i++)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromHours(1));
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cts.Token, cancellationToken);

            tasks.Add(Task.Run(()
                => Task.Delay(delayMs, linkedCts.Token), linkedCts.Token));
        }

        await Task.WhenAll(tasks);
    }
}
