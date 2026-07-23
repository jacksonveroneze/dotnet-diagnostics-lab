using Asp.Versioning;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Exception;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Enums;
using Microsoft.AspNetCore.Mvc;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Endpoints.Exception.v1;

internal static class ExceptionEndpoint
{
    private const string Resource = "exception";
    private const int Version = 1;

    public static WebApplication AddExceptionEndpoints(
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

        builder.AddThrowEndpoint();

        return app;
    }

    extension(RouteGroupBuilder builder)
    {
        private RouteGroupBuilder AddThrowEndpoint()
        {
            builder.MapGet("throw", (
                    [FromServices] IExceptionSimulationService service,
                    ExceptionSimulationType type) =>
                {
                    service.Run(type);

                    return Results.Ok();
                })
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);
            return builder;
        }
    }
}
