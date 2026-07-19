using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;

public interface ILohPressureService
{
    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes);
}
