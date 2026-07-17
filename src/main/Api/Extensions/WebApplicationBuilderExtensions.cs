using JacksonVeroneze.NET.GRPCServer.Api.Configurations;
using JacksonVeroneze.NET.GRPCServer.Api.Middlewares;

namespace JacksonVeroneze.NET.GRPCServer.Api.Extensions;

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
                .AddCorrelation()
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
