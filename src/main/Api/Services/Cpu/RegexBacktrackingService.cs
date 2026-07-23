using System.Text.RegularExpressions;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Cpu;

public partial class RegexBacktrackingService : IRegexBacktrackingService
{
    private const int MinValue = 1;
    private const int MaxValue = 30;

    public SimulationResult Run(
        int inputLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(inputLength, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(inputLength, MaxValue);

        return SimulationRunner.Run(()
            => InternalRun(inputLength));
    }

    private static void InternalRun(int inputLength)
    {
        var input = new string('a', inputLength) + "!";

        VulnerablePattern().IsMatch(input);
    }

#pragma warning disable MA0009 // Intentional: simulates a regex vulnerable to catastrophic backtracking (ReDoS).
    [GeneratedRegex("^(a+)+$")]
    private static partial Regex VulnerablePattern();
#pragma warning restore MA0009
}
