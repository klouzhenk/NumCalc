using CSnakes.Runtime;

namespace NumCalc.Calculation.Api.HostedServices;

public sealed class PythonWarmingUpService(IPythonEnvironment env) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            var warmingUpService = env.WarmingUp();
            warmingUpService.Run();
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}