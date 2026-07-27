using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Io;

public interface IBlockingIoService
{
    public SimulationResult Run(
        int taskCount,
        int delayMs,
        string targetUrl);
}
