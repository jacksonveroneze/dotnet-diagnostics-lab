using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;

public interface ICacheLeakService
{
    public Task<SimulationResult> RunAsync(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken);
}
