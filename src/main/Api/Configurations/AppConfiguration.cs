using System.Diagnostics.CodeAnalysis;

namespace JacksonVeroneze.NET.GRPCServer.Api.Configurations;

[ExcludeFromCodeCoverage]
public sealed record AppConfiguration
{
    public bool Type { get; set; }
    
    public AppInfoConfiguration? Application { get; init; }

    public string AppName =>
        Application!.Name!;

    public Version AppVersion =>
        Application!.Version!;
}
