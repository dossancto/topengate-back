using System.Diagnostics.Metrics;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Opengate.Modules.Shared.Adapters.Monitoring;

public static class TelemetryConfiguration
{
    public static IServiceCollection AddMonitoring(this IServiceCollection services)
    {
        var appName = nameof(Opengate);

        var serviceName = AppEnv.OTEL_SERVICE_NAME.GetDefault(appName);

        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";

        services.AddOpenTelemetry()
            .UseOtlpExporter()
            .ConfigureResource(resourceBuilder => resourceBuilder
                    .AddService(serviceName: serviceName,
                                serviceVersion: serviceVersion))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource(appName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    ;
            })
            .WithMetrics(meterProviderBuilder => meterProviderBuilder
                    .AddMeter(appName)
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddView(instrument =>
                    {
                        return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                            ? new Base2ExponentialBucketHistogramConfiguration()
                            : null;
                    }));

        services.AddLogging(options =>
            {
                options.AddOpenTelemetry(logOptions =>
                    {
                        logOptions.IncludeScopes = true;
                        logOptions.ParseStateValues = true;
                        logOptions.IncludeFormattedMessage = true;
                    });
            }
            );

        return services;
    }
}