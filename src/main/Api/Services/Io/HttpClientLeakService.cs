using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Io;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Io;

public class HttpClientLeakService : IHttpClientLeakService
{
    private const int MinRequestCount = 1;
    private const int MaxRequestCount = 1_000;

    private const string TargetUri = "http://localhost:7000/health";
    private readonly Uri _targetUriInstance = new(TargetUri, UriKind.RelativeOrAbsolute);

    public async Task<SimulationResult> RunAsync(
        int requestCount,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(requestCount, MinRequestCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(requestCount, MaxRequestCount);

        return await SimulationRunner.RunAsync(()
            => InternalRunAsync(_targetUriInstance,
                requestCount, cancellationToken));
    }

    private static async Task InternalRunAsync(
        Uri targetUri,
        int requestCount,
        CancellationToken cancellationToken)
    {
        var tasks = Enumerable.Range(0, requestCount)
            .Select(_ => SendLeakyRequestAsync(
                targetUri, cancellationToken))
            .ToList();

        await Task.WhenAll(tasks);
    }

    private static async Task SendLeakyRequestAsync(
        Uri targetUri,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        using HttpResponseMessage response =
            await httpClient.GetAsync(targetUri, cancellationToken);
    }
}
