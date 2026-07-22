using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;
using Timer = System.Timers.Timer;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class ClosureLeakService : IClosureLeakService
{
    private const int MinObjectCount = 1;
    private const int MaxObjectCount = 10_000;
    private const int MinObjectSizeBytes = 1;
    private const int MaxObjectSizeBytes = 1_048_576;
    private const double IntervalMs = 60_000;

    private static readonly List<DataProcessor> Processors = [];
    private static readonly Lock ProcessorsLock = new();

    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(objectCount, MinObjectCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectCount, MaxObjectCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(objectSizeBytes, MinObjectSizeBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectSizeBytes, MaxObjectSizeBytes);

        return SimulationRunner.Run(()
            => InternalRun(objectCount, objectSizeBytes));
    }

    private static void InternalRun(
        int objectCount,
        int objectSizeBytes)
    {
        for (var i = 0; i < objectCount; i++)
        {
            var processor = new DataProcessor(objectSizeBytes, IntervalMs);

            lock (ProcessorsLock)
            {
                Processors.Add(processor);
            }
        }
    }

#pragma warning disable CA1001 // Intentional: simulates a Timer-owning type that is never disposed.
    private sealed class DataProcessor
    {
        private readonly byte[] _largeData;
        private readonly Timer _timer;

        public DataProcessor(int dataSizeBytes, double intervalMs)
        {
            _largeData = new byte[dataSizeBytes];
            _timer = new Timer(intervalMs);
            _timer.Elapsed += (_, _) => { _ = _largeData.Length; };
            _timer.Start();
        }

#pragma warning disable S1144 // Intentional: never invoked, matches the timer-never-stopped scenario being simulated.
        public void Stop()
        {
            _timer.Stop();
        }
#pragma warning restore S1144
    }
#pragma warning restore CA1001
}
