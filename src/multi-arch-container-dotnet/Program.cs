using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

//1) Configuration sources, in ascending order of precedence:
//     appsettings.json -> appsettings.{Environment}.json -> environment variables -> command line.
//   Host.CreateApplicationBuilder wires all four up for us.
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
        loggerConfig.WriteTo.Console(new JsonFormatter());
    else
        loggerConfig.WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true);
});

//4) TimeProvider keeps the worker's delays deterministic and testable.
builder.Services.AddSingleton(TimeProvider.System);

//5) The worker itself. The host traps SIGINT/SIGTERM and cancels the stopping token, so both
//   `docker stop` and `kubectl delete pod` shut the application down cleanly.
builder.Services.AddHostedService<WorkerService>();

var host = builder.Build();

host.Services.GetRequiredService<ILogger<Program>>().LogInformation("Hit Ctrl-C to exit....");

await host.RunAsync();
