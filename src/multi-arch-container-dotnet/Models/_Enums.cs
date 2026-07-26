namespace CasCap.Models;

/// <summary>Console log output format.</summary>
public enum LogFormat
{
    /// <summary>Human-readable, coloured console output. Best for local development.</summary>
    Text = 0,

    /// <summary>Newline-delimited JSON. Best for log shipping (Loki, Elasticsearch, OpenTelemetry).</summary>
    Json = 1,
}
