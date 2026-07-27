using Asp.Versioning;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Io;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Endpoints.Io.v1;

internal static class IoEndpoint
{
    private const string Resource = "io";
    private const int Version = 1;

    public static WebApplication AddIoEndpoints(
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

        builder.AddHttpClientLeakEndpoint()
            .AddBlockingIoEndpoint()
            .AddDelayEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddHttpClientLeakEndpoint()
        {
            builder.MapGet("leak-http-client", async (
                    [FromServices] IHttpClientLeakService service,
                    int requestCount,
                    string targetUrl,
                    CancellationToken cancellationToken) =>
                {
                    if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out Uri? targetUri))
                    {
                        throw new ArgumentException(
                            "targetUrl must be an absolute URL.", nameof(targetUrl));
                    }

                    SimulationResult result = await service.RunAsync(
                        requestCount, targetUri, cancellationToken);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddBlockingIoEndpoint()
        {
            builder.MapGet("blocking-sync", (
                    [FromServices] IBlockingIoService service,
                    int taskCount,
                    int delayMs,
                    string targetUrl) =>
                {
                    SimulationResult result = service.Run(
                        taskCount, delayMs, targetUrl);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }

        private RouteGroupBuilder AddDelayEndpoint()
        {
            builder.MapGet("delay", async (
                    int delayMs,
                    CancellationToken cancellationToken) =>
                {
                    await Task.Delay(delayMs, cancellationToken);

                    return Results.Ok();
                });
            return builder;
        }
    }
}
