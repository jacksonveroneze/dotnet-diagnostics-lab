using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;

public interface IGen2PromotionService
{
    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes);
}
