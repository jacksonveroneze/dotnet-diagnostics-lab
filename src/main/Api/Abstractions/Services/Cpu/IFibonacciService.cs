using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;

public interface IFibonacciService
{
    SimulationResult Run(
        int n,
        SimulateType simulateType,
        CancellationToken cancellationToken);
}
