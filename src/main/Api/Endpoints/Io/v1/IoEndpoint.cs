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

        builder.AddHttpClientLeakEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddHttpClientLeakEndpoint()
        {
            builder.MapGet("leak-http-client", async (
                    [FromServices] IHttpClientLeakService service,
                    HttpContext httpContext,
                    int requestCount,
                    CancellationToken cancellationToken) =>
                {
                    SimulationResult result = await service.RunAsync(
                        requestCount, cancellationToken);

                    return Results.Ok(result);
                })
                .Produces<SimulationResult>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }
    }
}
