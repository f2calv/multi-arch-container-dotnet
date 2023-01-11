using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Runtime.InteropServices;

var loggerConfiguration = new LoggerConfiguration();

loggerConfiguration.WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true);
Log.Logger = loggerConfiguration
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .CreateLogger();

while (true)
{
    Log.Information("App '{appName}' on [Process Architecture: {arch}, OSArchitecture: {osArch}, OSDescription: {os}].",
        AppDomain.CurrentDomain.FriendlyName, RuntimeInformation.ProcessArchitecture, RuntimeInformation.OSArchitecture, RuntimeInformation.OSDescription);
    Log.Information("Git information; {@gitInfo}", new GitInfo());
    Log.Information("Github information; {@gitHubInfo}", new GitHubInfo());

    await Task.Delay(2_000, cancellationToken: CancellationToken.None);
}

public class GitInfo
{
    public GitInfo()
    {
        GIT_REPO = Environment.GetEnvironmentVariable("GIT_REPO") ?? "n/a";
        GIT_BRANCH = Environment.GetEnvironmentVariable("GIT_BRANCH") ?? "n/a";
        GIT_COMMIT = Environment.GetEnvironmentVariable("GIT_COMMIT") ?? "n/a";
        GIT_TAG = Environment.GetEnvironmentVariable("GIT_TAG");
    }
    public string? GIT_REPO { get; }
    public string? GIT_BRANCH { get; }
    public string? GIT_COMMIT { get; }
    public string? GIT_TAG { get; }
}

public class GitHubInfo
{
    public GitHubInfo()
    {
        GITHUB_WORKFLOW = Environment.GetEnvironmentVariable("GIT_REPO") ?? "n/a";
        GITHUB_RUN_ID = Environment.GetEnvironmentVariable("GIT_BRANCH") ?? "n/a";
        GITHUB_RUN_NUMBER = Environment.GetEnvironmentVariable("GIT_COMMIT") ?? "n/a";
    }
    public string? GITHUB_WORKFLOW { get; }
    public string? GITHUB_RUN_ID { get; }
    public string? GITHUB_RUN_NUMBER { get; }
}