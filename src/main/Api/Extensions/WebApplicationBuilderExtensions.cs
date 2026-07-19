using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Configurations;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Middlewares;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Extensions;

internal static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder Configure()
        {
            builder.Services.AddAppConfigs(builder.Configuration);

            var appConfiguration = builder.Configuration
                .Get<AppConfiguration>()!;

            builder.ConfigureDefaultServices(appConfiguration);

            //builder.AddLogger(appConfiguration);

            return builder;
        }

        private void ConfigureDefaultServices(
            AppConfiguration appConfiguration)
        {
            builder.Services.AddHybridCache();
            
            builder.Services
                .AddProblemDetails()
                .AddExceptionHandler<CustomExceptionHandler>()
                .AddJsonOptionsSerialize()
                .AddAppVersioning()
                .AddRouting()
                .AddApplicationServices()
                .AddOpenTelemetry(appConfiguration)
                .AddHealthChecks();

            if (!builder.Environment.IsProduction())
            {
                builder.Services
                    .AddOpenApi();
            }
        }
    }
}
