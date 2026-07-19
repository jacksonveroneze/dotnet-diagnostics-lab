using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;

public interface IStringAllocationService
{
    SimulationResult Run(
        int iterations,
        int stringLength);
}
