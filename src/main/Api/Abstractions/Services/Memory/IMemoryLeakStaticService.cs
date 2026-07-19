using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;

public interface IMemoryLeakStaticService
{
    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes,
        CancellationToken cancellationToken);
}
