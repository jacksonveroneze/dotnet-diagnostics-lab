using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Io;

public interface IHttpClientLeakService
{
    public Task<SimulationResult> RunAsync(
        int requestCount,
        CancellationToken cancellationToken);
}
