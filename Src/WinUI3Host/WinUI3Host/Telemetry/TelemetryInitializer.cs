using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Azure.Monitor.OpenTelemetry.Exporter;

namespace WinUI3Host.Telemetry
{
    public static class TelemetryInitializer
    {
        private static TracerProvider? _tracerProvider;
        private static MeterProvider? _meterProvider;

        public static void Initialize(string serviceName, string connectionString)
        {
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddSource(serviceName) // Ensures traces are captured from this app and any library using the same source
                .AddAzureMonitorTraceExporter(o => o.ConnectionString = connectionString)
                .Build();

            _meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddMeter(serviceName)
                .AddAzureMonitorMetricExporter(o => o.ConnectionString = connectionString)
                .Build();
        }

        public static void Shutdown()
        {
            _tracerProvider?.Dispose();
            _meterProvider?.Dispose();
        }
    }
}