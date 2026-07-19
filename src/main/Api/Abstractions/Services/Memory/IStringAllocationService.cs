using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;

public interface IStringAllocationService
{
    SimulationResult Run(
        int iterations,
        int stringLength,
        CancellationToken cancellationToken);
}
