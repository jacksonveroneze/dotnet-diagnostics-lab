namespace JacksonVeroneze.NET.GRPCServer.Api.Models;

public record GcCount(int Gen0, int Gen1, int Gen2);

public record SimulationResult(
    string Endpoint,
    long DurationMs,
    long AllocatedBytes,
    GcCount GcCountBefore,
    GcCount GcCountAfter,
    IReadOnlyDictionary<string, object> Metadata
);
