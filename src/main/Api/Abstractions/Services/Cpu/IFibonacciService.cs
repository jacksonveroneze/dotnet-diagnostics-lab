using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Cpu;

public interface IFibonacciService
{
    SimulationResult Run(
        int sequencePosition);
}
