using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;

public interface ITimerLeakService
{
    public SimulationResult Run(
        int timerCount,
        int intervalMs);
}
