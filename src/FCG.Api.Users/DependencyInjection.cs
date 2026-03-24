using FCG.Lib.Shared.Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;

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

        var swaggerBasePath = configuration["SWAGGER_BASE_PATH"];
        if (!string.IsNullOrEmpty(swaggerBasePath))
            services.AddSwaggerGen(c => c.AddServer(new OpenApiServer { Url = swaggerBasePath }));

        services.AddDefaultCors();

        return services;
    }
}
