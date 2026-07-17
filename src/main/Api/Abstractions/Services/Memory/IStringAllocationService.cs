using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;

public interface IStringAllocationService
{
    SimulationResult Run(
        int iterations,
        int stringLength,
        SimulateType simulateType,
        CancellationToken cancellationToken);
}
