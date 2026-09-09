namespace CasCap.Services;

/// <summary>Background worker that periodically logs runtime, configuration and build provenance information.</summary>
/// <remarks>
/// Exists purely to demonstrate the wiring: structured logging via <see cref="ILogger{TCategoryName}"/>
/// (Serilog behind the scenes) and strongly-typed configuration via <see cref="IOptions{TOptions}"/>.
/// The sibling Go, Rust and Python repositories contain a functionally identical worker.
/// </remarks>
public sealed partial class WorkerService(
    ILogger<WorkerService> logger,
    IOptions<AppConfig> appConfig,
    IOptions<BuildInfo> buildInfo,
    TimeProvider timeProvider
    ) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(logger, nameof(WorkerService), appConfig.Value.Greeting,
            appConfig.Value.IntervalSeconds, appConfig.Value.LogFormat);

        var interval = TimeSpan.FromSeconds(appConfig.Value.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("worker.iteration");

            LogRuntime(logger, nameof(WorkerService), appConfig.Value.Greeting,
                AppDomain.CurrentDomain.FriendlyName,
                RuntimeInformation.ProcessArchitecture, RuntimeInformation.OSArchitecture,
                RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription);

            LogGitProvenance(logger, nameof(WorkerService), buildInfo.Value.GitRepository,
                buildInfo.Value.GitBranch, buildInfo.Value.GitCommit, buildInfo.Value.GitTag);

            LogGitHubProvenance(logger, nameof(WorkerService), buildInfo.Value.GitHubWorkflow,
                buildInfo.Value.GitHubRunId, buildInfo.Value.GitHubRunNumber);

            Telemetry.WorkerIterations.Add(1);

            try
            {
                await Task.Delay(interval, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        LogStopping(logger, nameof(WorkerService));
    }

    [LoggerMessage(LogLevel.Information,
        "{ClassName} started, Greeting={Greeting} IntervalSeconds={IntervalSeconds} LogFormat={LogFormat}")]
    private static partial void LogStarted(
        ILogger logger,
        string className,
        string greeting,
        int intervalSeconds,
        LogFormat logFormat);

    [LoggerMessage(LogLevel.Information,
        "{ClassName} {Greeting} AppName={AppName} ProcessArchitecture={ProcessArchitecture} OSArchitecture={OSArchitecture} OSDescription={OSDescription} FrameworkDescription={FrameworkDescription}")]
    private static partial void LogRuntime(
        ILogger logger,
        string className,
        string greeting,
        string appName,
        Architecture processArchitecture,
        Architecture osArchitecture,
        string osDescription,
        string frameworkDescription);

    [LoggerMessage(LogLevel.Information,
        "{ClassName} git provenance, Repository={GitRepository} Branch={GitBranch} Commit={GitCommit} Tag={GitTag}")]
    private static partial void LogGitProvenance(
        ILogger logger,
        string className,
        string gitRepository,
        string gitBranch,
        string gitCommit,
        string gitTag);

    [LoggerMessage(LogLevel.Information,
        "{ClassName} github provenance, Workflow={GitHubWorkflow} RunId={GitHubRunId} RunNumber={GitHubRunNumber}")]
    private static partial void LogGitHubProvenance(
        ILogger logger,
        string className,
        string gitHubWorkflow,
        string gitHubRunId,
        string gitHubRunNumber);

    [LoggerMessage(LogLevel.Information, "{ClassName} stopping")]
    private static partial void LogStopping(ILogger logger, string className);
}
