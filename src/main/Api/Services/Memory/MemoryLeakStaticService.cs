using System.Diagnostics;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Helpers;
using JacksonVeroneze.NET.GRPCServer.Api.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace JacksonVeroneze.NET.GRPCServer.Api.Services.Memory;

public class MemoryLeakStaticService(HybridCache cache) : IMemoryLeakStaticService
{
    // private static readonly List<byte[]> _leakedObjects = [];

    public SimulationResult Run()
    {
        var gcBefore = GcMetrics.CollectionCount();

        var bytesBefore = GcMetrics.TotalAllocatedBytes();

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 25; i++)
        {
            _ = cache.SetAsync<string>(
                $"key:{Guid.NewGuid()}",
                $"{Guid.NewGuid()}",
                new HybridCacheEntryOptions()
                {
                    Expiration = TimeSpan.FromHours(1)
                }
            );
        }
        // _ = cache.SetAsync<string>(
        //     $"{Guid.NewGuid()}",
        //     $"{Guid.NewGuid()}",
        //     new HybridCacheEntryOptions()
        //     {
        //         Expiration = TimeSpan.FromHours(1)
        //     }
        // );

        // for (var i = 0; i < 25; i++)
        // {
        //     _leakedObjects.Add(new byte[5 * 1024]);
        // }

        stopwatch.Stop();

        var bytesAfter = GcMetrics.TotalAllocatedBytes();
        var gcAfter = GcMetrics.CollectionCount();

        return new SimulationResult(
            DurationMs: stopwatch.ElapsedMilliseconds,
            AllocatedBytes: bytesAfter - bytesBefore,
            GcCountBefore: gcBefore,
            GcCountAfter: gcAfter,
            Iterations: 0
        );
    }
}
