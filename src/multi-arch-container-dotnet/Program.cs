using CasCap.Models;
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
    Log.Information("App '{AppName}' on [Process Architecture: {ProcessArchitecture}, OSArchitecture: {OSArchitecture}, OSDescription: {OSDescription}].",
        AppDomain.CurrentDomain.FriendlyName, RuntimeInformation.ProcessArchitecture, RuntimeInformation.OSArchitecture, RuntimeInformation.OSDescription);

    Log.Information("Git information; name '{GIT_REPOSITORY}', branch '{GIT_BRANCH}', commit '{GIT_COMMIT}', tag '{GIT_TAG}'",
        appSettings.GIT_REPOSITORY, appSettings.GIT_BRANCH, appSettings.GIT_COMMIT, appSettings.GIT_TAG);

    Log.Information("GitHub information; workflow '{GITHUB_WORKFLOW}', run id '{GITHUB_RUN_ID}', run number '{GITHUB_RUN_NUMBER}'",
        appSettings.GITHUB_WORKFLOW, appSettings.GITHUB_RUN_ID, appSettings.GITHUB_RUN_NUMBER);

    await Task.Delay(3_000, cancellationToken: CancellationToken.None);
}
