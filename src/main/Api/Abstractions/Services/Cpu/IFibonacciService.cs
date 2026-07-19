using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;

public interface IFibonacciService
{
    SimulationResult Run(
        int sequencePosition,
        CancellationToken cancellationToken);
}
