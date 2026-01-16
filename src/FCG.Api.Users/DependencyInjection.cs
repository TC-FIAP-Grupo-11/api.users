using FCG.Lib.Shared.Infrastructure.DependencyInjection;

namespace FCG.Api.Users;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        services.AddJwtAuthentication(configuration);
        services.AddDefaultAuthorization();

        services.AddSwaggerWithJwt("FCG Users API", "v1");

        services.AddDefaultCors();

        return services;
    }
}
