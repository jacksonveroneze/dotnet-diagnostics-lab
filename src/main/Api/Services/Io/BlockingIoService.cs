using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Io;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Io;

public class BlockingIoService : IBlockingIoService
{
    private const int MinTaskCount = 1;
    private const int MaxTaskCount = 10;
    private const int MinDelayMs = 1;
    private const int MaxDelayMs = 10_000;

    private const string DelayEndpointBaseUrl = "http://localhost:7000/diagnostics/v1/io/delay";

    public SimulationResult Run(
        int taskCount,
        int delayMs)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskCount, MinTaskCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taskCount, MaxTaskCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(delayMs, MinDelayMs);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delayMs, MaxDelayMs);

        return SimulationRunner.Run(()
            => InternalRun(taskCount, delayMs));
    }

    private static void InternalRun(
        int taskCount, int delayMs)
    {
        var targetUri = new Uri($"{DelayEndpointBaseUrl}?delayMs={delayMs}");

        var tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() => BlockOnHttpCall(targetUri)));

        // Anti-pattern intencional: espera síncrona sobre as tasks, refletindo o mesmo
        // bloqueio de thread do ThreadPool que cada uma delas já faz internamente.
        Task.WaitAll(tasks);
    }

    private static void BlockOnHttpCall(
        Uri targetUri)
    {
        using var httpClient = new HttpClient();

        // Anti-pattern intencional: bloqueia a worker thread do ThreadPool em I/O síncrono
        // (sync-over-async) em vez de manter o pipeline assíncrono ponta a ponta.
        using HttpResponseMessage response = httpClient
            .GetAsync(targetUri)
            .GetAwaiter()
            .GetResult();
    }
}
