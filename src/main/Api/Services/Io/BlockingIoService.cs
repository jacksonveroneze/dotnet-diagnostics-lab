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

    public SimulationResult Run(
        int taskCount,
        int delayMs,
        string targetUrl)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskCount, MinTaskCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(taskCount, MaxTaskCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(delayMs, MinDelayMs);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delayMs, MaxDelayMs);

        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out Uri? targetUri))
        {
            throw new ArgumentException(
                "targetUrl must be an absolute URL.", nameof(targetUrl));
        }

        var callUri = new Uri($"{targetUri}?delayMs={delayMs}");

        return SimulationRunner.Run(()
            => InternalRun(taskCount, callUri));
    }

    private static void InternalRun(
        int taskCount,
        Uri callUri)
    {
        var tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() => BlockOnHttpCall(callUri)))
            .ToArray();

        // Anti-pattern intencional: espera síncrona sobre as tasks, refletindo o mesmo
        // bloqueio de thread do ThreadPool que cada uma delas já faz internamente.
        Task.WaitAll(tasks);
    }

    private static void BlockOnHttpCall(
        Uri targetUri)
    {
        using var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        // Anti-pattern intencional: bloqueia a worker thread do ThreadPool em I/O síncrono
        // (sync-over-async) em vez de manter o pipeline assíncrono ponta a ponta.
        using HttpResponseMessage response = httpClient
            .GetAsync(targetUri)
            .GetAwaiter()
            .GetResult();
    }
}
