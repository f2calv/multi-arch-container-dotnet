using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

//1) Configuration sources, in ascending order of precedence:
//     appsettings.json -> appsettings.{Environment}.json -> environment variables -> command line.
//   Host.CreateApplicationBuilder wires all four up for us.
//   The environment-specific file is selected by DOTNET_ENVIRONMENT rather than the APP_* naming
//   used elsewhere, because the host owns that variable. The sibling repositories deliberately do
//   NOT hand-roll an equivalent - a built-in is not worth reimplementing three times.
var builder = Host.CreateApplicationBuilder(args);

//2) Strongly-typed, validated configuration.
//   AppConfig binds the hierarchical "App" section - override any value with the standard
//   double-underscore syntax, e.g. App__IntervalSeconds=10.
//   BuildInfo binds the flat, unprefixed provenance variables baked into the image (GIT_TAG etc).
builder.Services.AddOptions<AppConfig>()
    .Bind(builder.Configuration.GetSection(AppConfig.SectionKey))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<BuildInfo>()
    .Bind(builder.Configuration);

//3) Structured logging. Serilog owns the Microsoft.Extensions.Logging pipeline, so application
//   code only ever depends on ILogger<T> - swapping Serilog out would not touch a single service.
//   Minimum levels and overrides are read from the "Serilog" section of appsettings.json.
var logFormat = builder.Configuration.GetValue(AppConfig.LogFormatKey, LogFormat.Text);

builder.Services.AddSerilog((services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName();

    if (logFormat == LogFormat.Json)
        loggerConfig.WriteTo.Console(new JsonFormatter(renderMessage: true));
    else
        loggerConfig.WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true);
});

//4) OpenTelemetry is opt-in. The standard OTEL_* variables configure resource attributes and
//   signal-specific exporter settings; console logging remains independently enabled above.
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (!string.IsNullOrWhiteSpace(otlpEndpoint))
{
    var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "multi-arch-container-dotnet";
    var serviceVersion = builder.Configuration["GIT_TAG"] is { Length: > 0 } tag && tag != BuildInfo.Unknown
        ? tag
        : null;

    builder.Services.Configure<OpenTelemetryLoggerOptions>(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddEnvironmentVariableDetector()
            .AddService(serviceName, serviceVersion: serviceVersion))
        .WithLogging()
        .WithTracing(tracing => tracing.AddSource(Telemetry.InstrumentationName))
        .WithMetrics(metrics => metrics.AddMeter(Telemetry.InstrumentationName))
        //This overload is the only way to pin the protocol to HTTP/protobuf, as the sibling
        //repositories do. A malformed endpoint surfaces as the SDK's own UriFormatException.
        .UseOtlpExporter(OtlpExportProtocol.HttpProtobuf, new Uri(otlpEndpoint));
}

//5) TimeProvider keeps the worker's delays deterministic and testable.
builder.Services.AddSingleton(TimeProvider.System);

//6) The worker itself. The host traps SIGINT/SIGTERM and cancels the stopping token, so both
//   `docker stop` and `kubectl delete pod` shut the application down cleanly.
builder.Services.AddHostedService<WorkerService>();

var host = builder.Build();

host.Services.GetRequiredService<ILogger<Program>>().LogInformation("Hit Ctrl-C to exit....");

await host.RunAsync();
