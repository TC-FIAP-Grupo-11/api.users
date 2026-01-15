namespace FCG.Api.Users.Contracts.Responses;

public record SignInResponse(
    string AccessToken,
    string IdToken,
    string RefreshToken,
    int ExpiresIn
);
