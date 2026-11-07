using Serilog;
using Serilog.Core;

namespace CasCap.Services;

/// <summary>Applies reloadable logging thresholds without discovering assemblies or configuration methods.</summary>
public sealed class SerilogLevelService : IDisposable
{
    private readonly LoggingLevelSwitch _defaultLevel;
    private readonly Dictionary<string, LoggingLevelSwitch> _overrides;
    private readonly IDisposable? _reloadSubscription;

    /// <summary>Creates switches for the configured default and existing category overrides.</summary>
    /// <param name="config">Logging threshold configuration and reload notifications.</param>
    public SerilogLevelService(IOptionsMonitor<LoggingLevelConfig> config)
    {
        _defaultLevel = new LoggingLevelSwitch(config.CurrentValue.Default);
        _overrides = config.CurrentValue.Override.ToDictionary(
            entry => entry.Key,
            entry => new LoggingLevelSwitch(entry.Value),
            StringComparer.Ordinal);
        _reloadSubscription = config.OnChange(UpdateLevels);
    }

    /// <summary>Attaches the configured switches to the logger.</summary>
    /// <param name="configuration">Logger being configured.</param>
    public void Configure(LoggerConfiguration configuration)
    {
        configuration.MinimumLevel.ControlledBy(_defaultLevel);
        foreach (var (category, levelSwitch) in _overrides)
            configuration.MinimumLevel.Override(category, levelSwitch);
    }

    /// <inheritdoc/>
    public void Dispose() => _reloadSubscription?.Dispose();

    private void UpdateLevels(LoggingLevelConfig config)
    {
        _defaultLevel.MinimumLevel = config.Default;
        foreach (var (category, levelSwitch) in _overrides)
            if (config.Override.TryGetValue(category, out var level))
                levelSwitch.MinimumLevel = level;
    }
}
