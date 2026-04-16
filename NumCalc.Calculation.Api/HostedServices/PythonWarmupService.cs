using CSnakes.Runtime;
using Microsoft.Extensions.Logging;

namespace NumCalc.Calculation.Api.HostedServices;

public sealed class PythonWarmingUpService(IPythonEnvironment env, ILogger<PythonWarmingUpService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Python warmup starting");
        await Task.Run(() =>
        {
            var warmingUpService = env.WarmingUp();
            warmingUpService.Run();
        }, cancellationToken);
        logger.LogInformation("Python warmup completed");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}