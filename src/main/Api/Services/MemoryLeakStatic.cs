using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services;

public class MemoryLeakStatic : IMemoryLeakStatic
{
    public SimulationResult Run()
    {
        throw new NotImplementedException();
    }
}
