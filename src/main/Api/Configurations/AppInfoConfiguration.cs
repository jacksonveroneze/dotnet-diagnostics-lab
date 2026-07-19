using System.Diagnostics.CodeAnalysis;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Configurations;

[ExcludeFromCodeCoverage]
public sealed record AppInfoConfiguration
{
    public string? Name { get; init; }

    public Version? Version { get; init; }
}
