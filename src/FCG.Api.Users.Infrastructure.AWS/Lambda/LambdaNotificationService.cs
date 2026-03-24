using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using FCG.Api.Users.Application.Contracts.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FCG.Api.Users.Infrastructure.AWS.Lambda;

public class LambdaNotificationService : ILambdaNotificationService
{
    private readonly IAmazonLambda _lambdaClient;
    private readonly string _functionName;
    private readonly ILogger<LambdaNotificationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LambdaNotificationService(
        IAmazonLambda lambdaClient,
        IConfiguration configuration,
        ILogger<LambdaNotificationService> logger)
    {
        _lambdaClient = lambdaClient;
        _functionName = configuration["AWS:NotificationLambdaName"] ?? "fcg-notification-sender";
        _logger = logger;
    }

    public async Task InvokeAsync<T>(string eventType, T payload, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            EventType = eventType,
            Payload = payload
        };

        var invokeRequest = new InvokeRequest
        {
            FunctionName = _functionName,
            InvocationType = InvocationType.Event, // assíncrono (fire and forget)
            Payload = JsonSerializer.Serialize(request, JsonOptions)
        };

        _logger.LogInformation("Invoking Lambda {FunctionName} with event type {EventType}", _functionName, eventType);

        await _lambdaClient.InvokeAsync(invokeRequest, cancellationToken);
    }
}
