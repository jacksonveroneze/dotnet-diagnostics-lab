using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class BlockingGcService : IBlockingGcService
{
    private const int MinIterations = 1;
    private const int MaxIterations = 1_000;
    private const int MinSurvivorCount = 1;
    private const int MaxSurvivorCount = 10_000;

    private const int SurvivorSizeBytes = 20_000;
    private const int GarbageSizeBytes = 20_000;
    private const int GarbageCountPerIteration = 200;

    public SimulationResult Run(
        int iterations,
        int survivorCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, MinIterations);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, MaxIterations);
        ArgumentOutOfRangeException.ThrowIfLessThan(survivorCount, MinSurvivorCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(survivorCount, MaxSurvivorCount);

        return SimulationRunner.Run(()
            => InternalRun(iterations, survivorCount));
    }

    private static void InternalRun(int iterations, int survivorCount)
    {
        List<byte[]> survivors = new(survivorCount);

        for (var i = 0; i < survivorCount; i++)
        {
            survivors.Add(new byte[SurvivorSizeBytes]);
        }

        for (var iteration = 0; iteration < iterations; iteration++)
        {
            AllocateShortLivedGarbage();

            // Anti-pattern intencional: os sobreviventes ficam retidos durante toda a
            // simulação; ao forçar uma coleta completa e bloqueante a cada iteração, o GC
            // precisa varrer e promover repetidamente esses sobreviventes até a Gen2,
            // gerando pausas reais (stop-the-world) observáveis no dotTrace.
            GC.Collect(2, GCCollectionMode.Forced, blocking: true);

            TouchSurvivors(survivors);
        }
    }

    private static void AllocateShortLivedGarbage()
    {
        for (var i = 0; i < GarbageCountPerIteration; i++)
        {
            var garbage = new byte[GarbageSizeBytes];
            garbage[0] = 1;
        }
    }

    private static void TouchSurvivors(List<byte[]> survivors)
    {
        foreach (var survivor in survivors)
        {
            survivor[0] = 1;
        }
    }
}
