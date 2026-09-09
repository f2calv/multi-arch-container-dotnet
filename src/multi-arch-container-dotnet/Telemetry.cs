using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CasCap;

/// <summary>Defines the application's OpenTelemetry instrumentation sources.</summary>
internal static class Telemetry
{
    /// <summary>Instrumentation scope shared by traces and metrics.</summary>
    public const string InstrumentationName = "io.github.f2calv.multi-arch-container-dotnet";

    /// <summary>Source for worker activities.</summary>
    public static readonly ActivitySource ActivitySource = new(InstrumentationName);

    /// <summary>Meter for worker instruments.</summary>
    public static readonly Meter Meter = new(InstrumentationName);

    /// <summary>Counts completed worker iterations.</summary>
    public static readonly Counter<long> WorkerIterations = Meter.CreateCounter<long>(
        "worker.iterations",
        unit: "{iteration}",
        description: "Number of completed worker iterations");
}