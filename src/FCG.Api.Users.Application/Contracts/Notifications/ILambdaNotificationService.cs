namespace FCG.Api.Users.Application.Contracts.Notifications;

public interface ILambdaNotificationService
{
    Task InvokeAsync<T>(string eventType, T payload, CancellationToken cancellationToken = default);
}
