namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

public record GcCount(int Gen0, int Gen1, int Gen2);

public record SimulationResult(
    long DurationMs,
    long AllocatedBytes,
    GcCount GcCountBefore,
    GcCount GcCountAfter
);
