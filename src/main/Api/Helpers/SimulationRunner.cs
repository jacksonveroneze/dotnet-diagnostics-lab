using System.Diagnostics;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;

internal static class SimulationRunner
{
    internal static SimulationResult Run(
        Action action)
    {
        var gcBefore = GcMetrics.CollectionCount();
        var bytesBefore = GcMetrics.TotalAllocatedBytes();
        var stopwatch = Stopwatch.StartNew();

        action();

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter
        );
    }

    internal static async Task<SimulationResult> RunAsync(Func<Task> action)
    {
        var gcBefore = GcMetrics.CollectionCount();
        var bytesBefore = GcMetrics.TotalAllocatedBytes();
        var stopwatch = Stopwatch.StartNew();

        await action();

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter
        );
    }
}
