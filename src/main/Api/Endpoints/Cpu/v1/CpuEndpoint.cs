using Asp.Versioning;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Cpu;
using JacksonVeroneze.NET.GRPCServer.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace JacksonVeroneze.NET.GRPCServer.Api.Endpoints.Cpu.v1;

internal static class CpuEndpoint
{
    private const string Resource = "cpu";
    private const int Version = 1;

    public static WebApplication AddCpuEndpoints(
        this WebApplication app)
    {
        var apiVersion = app.NewApiVersionSet()
            .ReportApiVersions()
            .HasApiVersion(
                new ApiVersion(Version))
            .Build();

        RouteGroupBuilder builder =
            app.MapGroup("diagnostics/v{version:apiVersion}/" + Resource)
                .WithTags(Resource)
                .WithApiVersionSet(apiVersion)
                .MapToApiVersion(Version);

        builder.AddFibonacciEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddFibonacciEndpoint()
        {
            builder.MapGet("fibonacci", (
                    [FromServices] IFibonacciService service,
                    int sequencePosition = 38,
                    CancellationToken cancellationToken = default) =>
                {
                    SimulationResult result = service.Run(
                        sequencePosition, cancellationToken);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }
    }
}
