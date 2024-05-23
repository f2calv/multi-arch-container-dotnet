using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Runtime.InteropServices;

var loggerConfiguration = new LoggerConfiguration();

loggerConfiguration.WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true);
Log.Logger = loggerConfiguration
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .CreateLogger();

var appSettings = new AppSettings();
while (true)
{
    Log.Information("App '{appName}' on [Process Architecture: {arch}, OSArchitecture: {osArch}, OSDescription: {os}].",
        AppDomain.CurrentDomain.FriendlyName, RuntimeInformation.ProcessArchitecture, RuntimeInformation.OSArchitecture, RuntimeInformation.OSDescription);

    Log.Information("Git information; name '{GIT_REPOSITORY}', branch '{GIT_BRANCH}', commit '{GIT_COMMIT}', tag '{GIT_TAG}'",
        appSettings.GIT_REPOSITORY, appSettings.GIT_BRANCH, appSettings.GIT_COMMIT, appSettings.GIT_TAG);

    Log.Information("GitHub information; workflow '{GITHUB_WORKFLOW}', run id '{GITHUB_RUN_ID}', run number '{GITHUB_RUN_NUMBER}'",
        appSettings.GITHUB_WORKFLOW, appSettings.GITHUB_RUN_ID, appSettings.GITHUB_RUN_NUMBER);

    await Task.Delay(3_000, cancellationToken: CancellationToken.None);
}

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