using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Cpu;

public class FibonacciService : IFibonacciService
{
    private const int MinValue = 1;
    private const int MaxValue = 40;

    public SimulationResult Run(
        int sequencePosition)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sequencePosition, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sequencePosition, MaxValue);

        return SimulationRunner.Run(()
            => InternalRun(sequencePosition));
    }

    private static long InternalRun(int sequencePosition)
    {
        return sequencePosition <= 1
            ? sequencePosition
            : InternalRun(sequencePosition - 1) +
              InternalRun(sequencePosition - 2);
    }
}
