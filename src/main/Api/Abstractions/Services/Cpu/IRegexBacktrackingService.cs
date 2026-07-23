using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Cpu;

public interface IRegexBacktrackingService
{
    SimulationResult Run(
        int inputLength);
}
