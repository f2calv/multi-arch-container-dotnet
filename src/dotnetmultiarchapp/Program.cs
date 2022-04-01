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

    Log.Information("Repository name '{GIT_REPO}', branch '{GIT_BRANCH}', commit '{GIT_COMMIT}', tag '{GIT_TAG}'",
        Environment.GetEnvironmentVariable("GIT_REPO"),
        Environment.GetEnvironmentVariable("GIT_BRANCH"),
        Environment.GetEnvironmentVariable("GIT_COMMIT"),
        Environment.GetEnvironmentVariable("GIT_TAG"));

    Log.Information("Github Action workflow name '{GITHUB_WORKFLOW}', run id '{GITHUB_RUN_ID}', run number '{GITHUB_RUN_NUMBER}'",
        Environment.GetEnvironmentVariable("GITHUB_WORKFLOW"),
        Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),
        Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER"));

    await Task.Delay(2_000, cancellationToken: CancellationToken.None);
}