namespace CasCap.Services;

/// <summary>Background worker that periodically logs runtime, configuration and build provenance information.</summary>
/// <remarks>
/// Exists purely to demonstrate the wiring: structured logging via <see cref="ILogger{TCategoryName}"/>
/// (Serilog behind the scenes) and strongly-typed configuration via <see cref="IOptions{TOptions}"/>.
/// The sibling Go and Rust repositories contain a functionally identical worker.
/// </remarks>
public sealed class WorkerService(
    ILogger<WorkerService> logger,
    IOptions<AppConfig> appConfig,
    IOptions<BuildInfo> buildInfo,
    TimeProvider timeProvider
    ) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{ClassName} started, Greeting={Greeting} IntervalSeconds={IntervalSeconds} LogFormat={LogFormat}",
            nameof(WorkerService), appConfig.Value.Greeting, appConfig.Value.IntervalSeconds, appConfig.Value.LogFormat);

        var interval = TimeSpan.FromSeconds(appConfig.Value.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("{ClassName} {Greeting} ProcessArchitecture={ProcessArchitecture} OSArchitecture={OSArchitecture} OSDescription={OSDescription} FrameworkDescription={FrameworkDescription}",
                nameof(WorkerService), appConfig.Value.Greeting, RuntimeInformation.ProcessArchitecture, RuntimeInformation.OSArchitecture,
                RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription);

            logger.LogInformation("{ClassName} git provenance, Repository={GitRepository} Branch={GitBranch} Commit={GitCommit} Tag={GitTag}",
                nameof(WorkerService), buildInfo.Value.GitRepository, buildInfo.Value.GitBranch, buildInfo.Value.GitCommit, buildInfo.Value.GitTag);

            logger.LogInformation("{ClassName} github provenance, Workflow={GitHubWorkflow} RunId={GitHubRunId} RunNumber={GitHubRunNumber}",
                nameof(WorkerService), buildInfo.Value.GitHubWorkflow, buildInfo.Value.GitHubRunId, buildInfo.Value.GitHubRunNumber);

            try
            {
                await Task.Delay(interval, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("{ClassName} stopping", nameof(WorkerService));
    }
}
