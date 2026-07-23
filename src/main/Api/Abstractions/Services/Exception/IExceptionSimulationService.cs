using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Enums;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Exception;

public interface IExceptionSimulationService
{
    void Run(
        ExceptionSimulationType type);
}
