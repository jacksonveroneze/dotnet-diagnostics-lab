using System.Reflection;

namespace JacksonVeroneze.NET.GRPCServer.Api;

public static class AssemblyReference
{
    public static readonly Assembly Assembly =
        typeof(AssemblyReference).Assembly;
}
