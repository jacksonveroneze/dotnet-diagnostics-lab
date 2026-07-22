using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;

public interface IEventLeakService
{
    public SimulationResult Run(
        int subscriberCount,
        int payloadSizeBytes);
}
