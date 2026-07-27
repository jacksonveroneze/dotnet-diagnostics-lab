using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;

public interface IBlockingGcService
{
    public SimulationResult Run(
        int iterations,
        int survivorCount);
}
