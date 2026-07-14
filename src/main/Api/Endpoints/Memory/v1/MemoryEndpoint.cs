using Asp.Versioning;
using JacksonVeroneze.NET.GRPCServer.Api.Abstractions.Services;
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
            app.MapGroup("disgnostics/v{version:apiVersion}/" + Resource)
                .WithTags(Resource)
                .WithApiVersionSet(apiVersion)
                .MapToApiVersion(Version);

        builder.AddStringAllocationEndpoint()
            .AddStaticLeakEndpoint()
            .AddGen2PromotionEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddStringAllocationEndpoint()
        {
            builder.MapGet("string-allocation", (
                    [FromServices] IMemoryStringAllocation service,
                    CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result = service.Run();

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddStaticLeakEndpoint()
        {
            builder.MapGet("leak-static", (
                    [FromServices] IMemoryLeakStatic service,
                    CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result = service.Run();

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddGen2PromotionEndpoint()
        {
            builder.MapGet("gen2-promotion", (
                    [FromServices] IMemoryG2PromotionService service,
                    CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result = service.Run();

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }
    }
}
