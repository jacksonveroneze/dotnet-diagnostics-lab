using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Thread;

public interface ILockContentionService
{
    Task<SimulationResult> RunAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken);
}
