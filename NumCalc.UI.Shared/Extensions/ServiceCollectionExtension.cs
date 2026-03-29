using Cropper.Blazor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace NumCalc.UI.Shared.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddNumCalcUiShared(this IServiceCollection services)
    {
        services.AddCropper();

        return services;
    }

    public static IServiceCollection AddSharedLogging(this IServiceCollection services, string logFilePath)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.File(
                path:logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            )
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        return services;
    }
}