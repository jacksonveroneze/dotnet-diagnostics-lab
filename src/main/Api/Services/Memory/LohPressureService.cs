using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class LohPressureService : ILohPressureService
{
    private const int MinObjectCount = 1;
    private const int MaxObjectCount = 2_000;
    private const int MinObjectSizeBytes = 85_000;
    private const int MaxObjectSizeBytes = 5_242_880;

    public SimulationResult Run(
        int objectCount,
        int objectSizeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(objectCount, MinObjectCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectCount, MaxObjectCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(objectSizeBytes, MinObjectSizeBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(objectSizeBytes, MaxObjectSizeBytes);

        return SimulationRunner.Run(()
            => InternalRun(objectCount, objectSizeBytes));
    }

    private static void InternalRun(
        int objectCount,
        int objectSizeBytes)
    {
        List<byte[]> retained = new(objectCount / 2);

        for (var i = 0; i < objectCount; i++)
        {
            var currentSizeBytes = i % 2 == 0
                ? objectSizeBytes
                : objectSizeBytes * 3 / 4;

            RetainBytes(retained, currentSizeBytes, i % 2 == 0);
        }
    }

    private static void RetainBytes(
        List<byte[]> retained,
        int currentSizeBytes,
        bool retain)
    {
        var buffer = new byte[currentSizeBytes];
        buffer[0] = 1;

        if (retain)
        {
            retained.Add(buffer);
        }
    }
}
