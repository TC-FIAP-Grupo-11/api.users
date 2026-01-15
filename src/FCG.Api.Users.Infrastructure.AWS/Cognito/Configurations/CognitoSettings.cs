namespace FCG.Api.Users.Infrastructure.AWS.Cognito.Configurations;

public class CognitoSettings
{
    public string UserPoolId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
