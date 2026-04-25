using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NumCalc.User.Application.Interfaces.Repositories;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Infrastructure.Data;
using NumCalc.User.Infrastructure.Repositories;
using NumCalc.User.Infrastructure.Services;

namespace NumCalc.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISavedInputRepository, SavedInputRepository>();
        services.AddScoped<ISavedFileRepository, SavedFileRepository>();
        services.AddScoped<ICalculationHistoryRepository, CalculationHistoryRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ISavedInputService, SavedInputService>();
        services.AddScoped<ISavedFileService, SavedFileService>();
        services.AddScoped<ICalculationHistoryService, CalculationHistoryService>();

        return services;
    }
}