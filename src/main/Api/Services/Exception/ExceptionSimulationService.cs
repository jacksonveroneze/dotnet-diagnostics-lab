using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Exception;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Enums;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Exception;

public class ExceptionSimulationService : IExceptionSimulationService
{
    public void Run(
        ExceptionSimulationType type)
    {
        switch (type)
        {
            case ExceptionSimulationType.Argument:
                throw new ArgumentException("Simulated argument exception.", nameof(type));
            case ExceptionSimulationType.Unhandled:
                throw new InvalidOperationException("Simulated unhandled exception.");
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}
