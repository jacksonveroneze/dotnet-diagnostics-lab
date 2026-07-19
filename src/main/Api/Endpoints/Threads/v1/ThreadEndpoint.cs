using Asp.Versioning;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Thread;
using JacksonVeroneze.NET.GRPCServer.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace JacksonVeroneze.NET.GRPCServer.Api.Endpoints.Threads.v1;

internal static class ThreadEndpoint
{
    private const string Resource = "thread";
    private const int Version = 1;

    public static WebApplication AddThreadEndpoints(
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

        builder.AddThreadPoolStarvationEndpoint()
            .AddThreadLeakEndpoint()
            .AddLockContentionEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddThreadPoolStarvationEndpoint()
        {
            builder.MapGet("thread-pool-starvation", async (
                    [FromServices] IThreadPoolStarvationService service,
                    int delayMs,
                    int taskCount,
                    CancellationToken cancellationToken = default) =>
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

        private RouteGroupBuilder AddThreadLeakEndpoint()
        {
            builder.MapGet("thread-leak", async (
                    [FromServices] IThreadLeakService service,
                    int delayMs,
                    int taskCount,
                    CancellationToken cancellationToken = default) =>
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

        private RouteGroupBuilder AddLockContentionEndpoint()
        {
            builder.MapGet("lock-contention", async (
                    [FromServices] ILockContentionService service,
                    int delayMs,
                    int taskCount,
                    CancellationToken cancellationToken = default) =>
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
    }
}
