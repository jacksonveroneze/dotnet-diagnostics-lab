using JacksonVeroneze.NET.GRPCServer.Api.Models;

namespace JacksonVeroneze.NET.GRPCServer.Api.Helpers;

internal static class GcMetrics
{
    internal static GcCount CollectionCount()
    {
        return new GcCount(
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2)
        );
    }

    internal static long TotalAllocatedBytes()
    {
        return GC.GetTotalAllocatedBytes();
    }
}
