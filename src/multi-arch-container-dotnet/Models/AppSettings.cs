namespace CasCap.Models;

public class AppSettings
{
    public AppSettings()
    {
        GIT_REPOSITORY = Environment.GetEnvironmentVariable(nameof(GIT_REPOSITORY)) ?? "n/a";
        GIT_BRANCH = Environment.GetEnvironmentVariable(nameof(GIT_BRANCH)) ?? "n/a";
        GIT_COMMIT = Environment.GetEnvironmentVariable(nameof(GIT_COMMIT)) ?? "n/a";
        GIT_TAG = Environment.GetEnvironmentVariable(nameof(GIT_TAG)) ?? "n/a";
        GITHUB_WORKFLOW = Environment.GetEnvironmentVariable(nameof(GITHUB_WORKFLOW)) ?? "n/a";
        GITHUB_RUN_ID = Environment.GetEnvironmentVariable(nameof(GITHUB_RUN_ID)) ?? "n/a";
        GITHUB_RUN_NUMBER = Environment.GetEnvironmentVariable(nameof(GITHUB_RUN_NUMBER)) ?? "n/a";
    }
    public string? GIT_REPOSITORY { get; }
    public string? GIT_BRANCH { get; }
    public string? GIT_COMMIT { get; }
    public string? GIT_TAG { get; }
    public string? GITHUB_WORKFLOW { get; }
    public string? GITHUB_RUN_ID { get; }
    public string? GITHUB_RUN_NUMBER { get; }
}
