using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Memory;

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

        for (var objectIndex = 0; objectIndex < objectCount; objectIndex++)
        {
            var currentSizeBytes = objectIndex % 2 == 0
                ? objectSizeBytes
                : objectSizeBytes * 3 / 4;

            var buffer = new byte[currentSizeBytes];
            buffer[0] = 1;

            if (objectIndex % 2 == 0)
            {
                retained.Add(buffer);
            }
        }
    }
}
