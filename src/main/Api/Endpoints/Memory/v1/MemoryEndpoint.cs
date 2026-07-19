using Asp.Versioning;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.GRPCServer.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace JacksonVeroneze.NET.GRPCServer.Api.Endpoints.Memory.v1;

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
            .AddGen2PromotionEndpoint()
            .AddLohPressureEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddStringAllocationEndpoint()
        {
            builder.MapGet("string-allocation", (
                    [FromServices] IStringAllocationService service,
                    int iterations = 10_000,
                    int stringLength = 1_000) =>
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
                    int objectCount = 1_000,
                    int objectSizeBytes = 10_000) =>
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

        private RouteGroupBuilder AddGen2PromotionEndpoint()
        {
            builder.MapGet("gen2-promotion", (
                    [FromServices] IGen2PromotionService service,
                    int objectCount = 1_000,
                    int objectSizeBytes = 10_000) =>
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
                    int objectCount = 200,
                    int objectSizeBytes = 100_000) =>
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
    }
}
