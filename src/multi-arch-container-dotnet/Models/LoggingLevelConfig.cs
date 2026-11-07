using Serilog.Events;

namespace CasCap.Models;

/// <summary>Configures the default logging threshold and category overrides.</summary>
public sealed record LoggingLevelConfig
{
    /// <summary>Configuration section containing the logging thresholds.</summary>
    public const string ConfigurationSectionName = "Serilog:MinimumLevel";

    /// <summary>Default threshold for application log events.</summary>
    public LogEventLevel Default { get; set; } = LogEventLevel.Information;

    /// <summary>Category-specific thresholds, keyed by source-context prefix.</summary>
    public Dictionary<string, LogEventLevel> Override { get; set; } = [];
}
