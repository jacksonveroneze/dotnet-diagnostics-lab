using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class StringAllocationService : IStringAllocationService
{
    private const int MinIterations = 1;
    private const int MaxIterations = 1_00_000;
    private const int MinStringLength = 1;
    private const int MaxStringLength = 99_000;

    public SimulationResult Run(
        int iterations,
        int stringLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, MinIterations);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, MaxIterations);
        ArgumentOutOfRangeException.ThrowIfLessThan(stringLength, MinStringLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stringLength, MaxStringLength);

        return SimulationRunner.Run(() 
            => InternalRun(iterations, stringLength));
    }

    private static void InternalRun(
        int iterations,
        int stringLength)
    {
        var chunk = new string('_', stringLength);
        
        var acc = string.Empty;

        for (var i = 0; i < iterations; i++)
        {
            acc += chunk + Environment.NewLine;
            acc += chunk + Environment.NewLine;
        }
    }
}
