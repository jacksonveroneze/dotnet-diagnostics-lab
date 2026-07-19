using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;

public interface ILockContentionService
{
    Task<SimulationResult> RunAsync(
        int delayMs,
        int taskCount,
        CancellationToken cancellationToken);
}
