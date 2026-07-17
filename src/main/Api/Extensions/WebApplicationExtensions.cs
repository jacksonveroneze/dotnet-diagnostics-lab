using JacksonVeroneze.NET.GRPCServer.Api.Endpoints.Cpu.v1;
using JacksonVeroneze.NET.GRPCServer.Api.Endpoints.Memory.v1;
using JacksonVeroneze.NET.GRPCServer.Api.Endpoints.Thread.v1;
using Scalar.AspNetCore;

namespace JacksonVeroneze.NET.GRPCServer.Api.Extensions;

internal static class WebApplicationExtensions
{
    private const string PathHealth = "/health";
    private const string PathMetrics = "metrics";
    
    public static WebApplication Configure(
        this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        
        app.UseRouting();

        app.UseHealthChecks(PathHealth);
        app.UseOpenTelemetryPrometheusScrapingEndpoint(PathMetrics);

        app.AddMemoryEndpoints();
        app.AddThreadEndpoints();
        app.AddCpuEndpoints();

        return app;
    }
}
