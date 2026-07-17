using JacksonVeroneze.NET.GRPCServer.Api.Enums;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;

public interface IThreadLeakService
{
    Task<SimulationResult> RunAsync(
        int delayMs,
        int taskCount,
        SimulateType simulateType,
        CancellationToken cancellationToken);
}
