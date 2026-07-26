namespace CasCap.Models;

/// <summary>Application configuration bound from the <c>app</c> section of <c>appsettings.json</c>.</summary>
/// <remarks>
/// Every property can be overridden by an environment variable using the standard ASP.NET Core
/// double-underscore section separator, e.g. <c>APP__INTERVAL_SECONDS=10</c>.
/// <para>
/// Keys are snake_case rather than PascalCase so that the file keys and environment variable names
/// are byte-identical across the sibling Go and Rust repositories, whose configuration libraries
/// lower-case environment keys. <see cref="ConfigurationKeyNameAttribute"/> maps each key onto an
/// idiomatic C# property name.
/// </para>
/// </remarks>
public sealed record AppConfig
{
    /// <summary>Configuration section name.</summary>
    public const string SectionKey = "app";

    /// <summary>Full configuration key for <see cref="LogFormat"/>, read before the logger exists.</summary>
    public const string LogFormatKey = $"{SectionKey}:log_format";

    /// <summary>Message written on every iteration of the worker loop.</summary>
    /// <remarks>Defaults to a generic greeting.</remarks>
    [ConfigurationKeyName("greeting")]
    [Required, MinLength(1)]
    public string Greeting { get; init; } = "Hello from a multi-architecture container";

    /// <summary>Delay between worker loop iterations, in seconds.</summary>
    /// <remarks>Defaults to 3 seconds.</remarks>
    [ConfigurationKeyName("interval_seconds")]
    [Range(1, 3600)]
    public int IntervalSeconds { get; init; } = 3;

    /// <inheritdoc cref="Models.LogFormat"/>
    /// <remarks>Defaults to <see cref="Models.LogFormat.Text"/>.</remarks>
    [ConfigurationKeyName("log_format")]
    public LogFormat LogFormat { get; init; } = LogFormat.Text;
}
