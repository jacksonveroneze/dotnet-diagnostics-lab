using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;

public interface IGen2PromotionService
{
    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes,
        SimulateType simulateType,
        CancellationToken cancellationToken);
}
