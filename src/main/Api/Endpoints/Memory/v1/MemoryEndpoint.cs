using Asp.Versioning;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Endpoints.Memory.v1;

internal static class MemoryEndpoint
{
    private const string Resource = "memory";
    private const int Version = 1;

    public static WebApplication AddMemoryEndpoints(
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

        builder.AddStringAllocationEndpoint()
            .AddStaticLeakEndpoint()
            .AddLohPressureEndpoint()
            .AddEventLeakEndpoint()
            .AddCacheLeakEndpoint()
            .AddClosureLeakEndpoint()
            .AddCancellationTokenSourceLeakEndpoint()
            .AddTimerLeakEndpoint()
            .AddGcCleanEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddStringAllocationEndpoint()
        {
            builder.MapGet("string-allocation", (
                    [FromServices] IStringAllocationService service,
                    int iterations,
                    int stringLength) =>
                {
                    SimulationResult result = service.Run(
                        iterations, stringLength);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddStaticLeakEndpoint()
        {
            builder.MapGet("leak-static", (
                    [FromServices] IMemoryLeakStaticService service,
                    int objectCount,
                    int objectSizeBytes) =>
                {
                    SimulationResult result = service.Run(
                        objectCount, objectSizeBytes);
                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddLohPressureEndpoint()
        {
            builder.MapGet("loh-pressure", (
                    [FromServices] ILohPressureService service,
                    int objectCount,
                    int objectSizeBytes) =>
                {
                    SimulationResult result = service.Run(
                        objectCount, objectSizeBytes);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddEventLeakEndpoint()
        {
            builder.MapGet("leak-event", (
                    [FromServices] IEventLeakService service,
                    int subscriberCount,
                    int payloadSizeBytes) =>
                {
                    SimulationResult result = service.Run(
                        subscriberCount, payloadSizeBytes);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddCacheLeakEndpoint()
        {
            builder.MapGet("leak-cache", async (
                    [FromServices] ICacheLeakService service,
                    int objectCount,
                    int objectSizeBytes,
                    CancellationToken cancellationToken) =>
                {
                    SimulationResult result = await service.RunAsync(
                        objectCount, objectSizeBytes, cancellationToken);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddClosureLeakEndpoint()
        {
            builder.MapGet("leak-closure", (
                    [FromServices] IClosureLeakService service,
                    int objectCount,
                    int objectSizeBytes) =>
                {
                    SimulationResult result = service.Run(
                        objectCount, objectSizeBytes);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddCancellationTokenSourceLeakEndpoint()
        {
            builder.MapGet("leak-cancellation-token-source", async (
                    [FromServices] ICancellationTokenSourceLeakService service,
                    int delayMs,
                    int taskCount,
                    CancellationToken cancellationToken) =>
                {
                    SimulationResult result = await service.RunAsync(
                        delayMs, taskCount, cancellationToken);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddTimerLeakEndpoint()
        {
            builder.MapGet("leak-timer", (
                    [FromServices] ITimerLeakService service,
                    int timerCount,
                    int intervalMs) =>
                {
                    SimulationResult result = service.Run(
                        timerCount, intervalMs);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }
        
        private RouteGroupBuilder AddGcCleanEndpoint()
        {
            builder.MapGet("gc-clean", () =>
                {
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

                    return Results.Ok();
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }
    }
}
