namespace CasCap.Models;

/// <summary>Build provenance baked into the container image at build time.</summary>
/// <remarks>
/// These are flat, unprefixed environment variables written by the <c>ARG</c>/<c>ENV</c> block of
/// the Dockerfile (populated by CI, or by <c>build.sh</c>/<c>build.ps1</c> locally). They use
/// SCREAMING_SNAKE_CASE rather than the hierarchical <c>Section__Property</c> convention, so each
/// property is mapped explicitly with <see cref="Microsoft.Extensions.Configuration.ConfigurationKeyNameAttribute"/>.
/// The sibling Go and Rust repositories consume the identical variable names.
/// </remarks>
public sealed record BuildInfo
{
    /// <summary>Git repository name, e.g. <c>f2calv/multi-arch-container-dotnet</c>.</summary>
    [ConfigurationKeyName("GIT_REPOSITORY")]
    public string GitRepository { get; init; } = Unknown;

    /// <summary>Git branch reference, e.g. <c>refs/heads/main</c>.</summary>
    [ConfigurationKeyName("GIT_BRANCH")]
    public string GitBranch { get; init; } = Unknown;

    /// <summary>Git commit SHA.</summary>
    [ConfigurationKeyName("GIT_COMMIT")]
    public string GitCommit { get; init; } = Unknown;

    /// <summary>Git tag, i.e. the semantic version of the image.</summary>
    [ConfigurationKeyName("GIT_TAG")]
    public string GitTag { get; init; } = Unknown;

    /// <summary>GitHub Actions workflow name.</summary>
    [ConfigurationKeyName("GITHUB_WORKFLOW")]
    public string GitHubWorkflow { get; init; } = Unknown;

    /// <summary>GitHub Actions run identifier.</summary>
    [ConfigurationKeyName("GITHUB_RUN_ID")]
    public string GitHubRunId { get; init; } = Unknown;

    /// <summary>GitHub Actions run number.</summary>
    [ConfigurationKeyName("GITHUB_RUN_NUMBER")]
    public string GitHubRunNumber { get; init; } = Unknown;

    /// <summary>Placeholder used when a provenance variable is absent, i.e. outside a container.</summary>
    public const string Unknown = "n/a";
}
