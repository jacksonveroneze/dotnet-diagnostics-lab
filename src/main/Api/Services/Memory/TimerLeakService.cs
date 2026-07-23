using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;
using Timer = System.Timers.Timer;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class TimerLeakService : ITimerLeakService
{
    private const int MinTimerCount = 1;
    private const int MaxTimerCount = 10_000;
    private const int MinIntervalMs = 1;
    private const int MaxIntervalMs = 3_600_000;

    private static readonly List<TimerHolder> Holders = [];
    private static readonly Lock HoldersLock = new();

    public SimulationResult Run(
        int timerCount,
        int intervalMs)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timerCount, MinTimerCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timerCount, MaxTimerCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(intervalMs, MinIntervalMs);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(intervalMs, MaxIntervalMs);

        return SimulationRunner.Run(()
            => InternalRun(timerCount, intervalMs));
    }

    private static void InternalRun(
        int timerCount,
        int intervalMs)
    {
        for (var i = 0; i < timerCount; i++)
        {
            var holder = new TimerHolder(intervalMs);

            holder.Start();

            lock (HoldersLock)
            {
                Holders.Add(holder);
            }
        }
    }

    private sealed class TimerHolder(double intervalMs)
    {
        private readonly Timer _timer = new(intervalMs);

        public void Start()
        {
            _timer.Start();
        }
    }
}
