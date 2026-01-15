using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Api.Users.Infrastructure.AWS.Cognito.Configurations;
using FCG.Api.Users.Infrastructure.AWS.Cognito.Services;
using FCG.Api.Users.Infrastructure.AWS.Seed;

namespace FCG.Api.Users.Infrastructure.AWS;

public static class DependencyInjection
{
    public static IServiceCollection AddAwsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AWS Settings
        var awsSettings = configuration.GetSection("AWS").Get<AwsSettings>() ?? new AwsSettings();

        // Bind AWS Settings
        services.Configure<AwsSettings>(options =>
            configuration.GetSection("AWS").Bind(options));

        // Bind Cognito Settings separately (if needed)
        services.Configure<CognitoSettings>(options =>
            configuration.GetSection("AWS:Cognito").Bind(options));

        // AWS Cognito Client
        services.AddSingleton<IAmazonCognitoIdentityProvider>(sp =>
        {
            AWSCredentials credentials;

            if (!string.IsNullOrEmpty(awsSettings.SessionToken))
            {
                credentials = new SessionAWSCredentials(
                    awsSettings.AccessKeyId,
                    awsSettings.SecretAccessKey,
                    awsSettings.SessionToken);
            }
            else if (!string.IsNullOrEmpty(awsSettings.AccessKeyId) && !string.IsNullOrEmpty(awsSettings.SecretAccessKey))
            {
                credentials = new BasicAWSCredentials(
                    awsSettings.AccessKeyId,
                    awsSettings.SecretAccessKey);
            }
            else
            {
                credentials = FallbackCredentialsFactory.GetCredentials();
            }

            return new AmazonCognitoIdentityProviderClient(
                credentials,
                RegionEndpoint.GetBySystemName(awsSettings.Region));
        });

        // Services
        services.AddScoped<IAuthenticationService, CognitoService>();
        
        // Seeder
        services.AddScoped<CognitoSeeder>();

        return services;
    }
}
