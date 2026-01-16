using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FCG.Api.Users.Application.Contracts.Repositories;
using FCG.Api.Users.Infrastructure.Data.Context;
using FCG.Api.Users.Infrastructure.Data.Repositories;
using FCG.Api.Users.Infrastructure.Data.Seed;
using FCG.Lib.Shared.Infrastructure.DependencyInjection;

namespace FCG.Api.Users.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabaseInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSqlServerDatabase<ApplicationDbContext>(configuration);
        services.AddRepositories();
        services.AddSeeders();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
    
    private static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
