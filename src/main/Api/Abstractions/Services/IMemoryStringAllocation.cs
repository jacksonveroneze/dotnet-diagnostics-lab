using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services;

public interface IMemoryStringAllocation
{
    public SimulationResult Run();
}
