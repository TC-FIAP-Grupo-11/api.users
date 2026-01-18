using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using FCG.Api.Users.Infrastructure.AWS.Cognito.Configurations;
using System.Security.Cryptography;
using System.Text;
using FCG.Api.Users.Domain.Entities;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Api.Users.Application.Contracts.Auth.Responses;
using FCG.Lib.Shared.Application.Common.Exceptions;
using CognitoInvalidPasswordException = Amazon.CognitoIdentityProvider.Model.InvalidPasswordException;

namespace FCG.Api.Users.Infrastructure.AWS.Cognito.Services;

public class CognitoService(
    IAmazonCognitoIdentityProvider cognitoClient,
    IOptions<CognitoSettings> settings) : IAuthenticationService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient = cognitoClient;
    private readonly CognitoSettings _settings = settings.Value;

    public async Task<string> SignUpAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var signUpRequest = new SignUpRequest
            {
                ClientId = _settings.ClientId,
                Username = user.Email,
                Password = password,
                SecretHash = GenerateSecretHash(user.Email),
                UserAttributes =
                [
                    new() { Name = "email", Value = user.Email },
                    new() { Name = "name", Value = user.Name },
                ]
            };

            var signUpResponse = await _cognitoClient.SignUpAsync(signUpRequest, cancellationToken);

            await AddUserToGroupAsync(user, cancellationToken);

            return signUpResponse.UserSub;
        }
        catch (UsernameExistsException ex)
        {
            throw new UserAlreadyExistsException(user.Email, ex);
        }
        catch (CognitoInvalidPasswordException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.InvalidPasswordException("Password does not meet security requirements", ex);
        }
        catch (TooManyRequestsException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.LimitExceededException("Too many sign up attempts. Please try again later", ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new UserAlreadyExistsException($"Sign up failed: {ex.Message}", ex);
        }
    }


    public async Task ConfirmSignUpAsync(string email, string confirmationCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmSignUpRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
                SecretHash = GenerateSecretHash(email)
            };

            await _cognitoClient.ConfirmSignUpAsync(request, cancellationToken);
        }
        catch (CodeMismatchException ex)
        {
            throw new InvalidConfirmationCodeException("Invalid confirmation code", ex);
        }
        catch (ExpiredCodeException ex)
        {
            throw new InvalidConfirmationCodeException("Confirmation code has expired", ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.UserNotFoundException(email, ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidConfirmationCodeException($"Failed to confirm sign up: {ex.Message}", ex);
        }
    }

    public async Task ResendConfirmationCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ResendConfirmationCodeRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
                SecretHash = GenerateSecretHash(email)
            };

            await _cognitoClient.ResendConfirmationCodeAsync(request, cancellationToken);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.UserNotFoundException(email, ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.LimitExceededException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.LimitExceededException("Too many requests. Please try again later", ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidConfirmationCodeException($"Failed to resend confirmation code: {ex.Message}", ex);
        }
    }

    public async Task<Token> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new InitiateAuthRequest
            {
                ClientId = _settings.ClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email },
                    { "PASSWORD", password },
                    { "SECRET_HASH", GenerateSecretHash(email) }
                }
            };

            var response = await _cognitoClient.InitiateAuthAsync(request, cancellationToken);

            return TokenFromAuthResult(response.AuthenticationResult);
        }
        catch (NotAuthorizedException ex)
        {
            throw new InvalidCredentialsException("Invalid email or password", ex);
        }
        catch (UserNotConfirmedException ex)
        {
            throw new EmailNotConfirmedException("Email address is not confirmed", ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new InvalidCredentialsException("Invalid email or password", ex);
        }
        catch (PasswordResetRequiredException ex)
        {
            throw new PasswordResetFailedException("Password reset is required", ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.InvalidPasswordException ex)
        {
            throw new InvalidCredentialsException("Invalid email or password", ex);
        }
        catch (TooManyRequestsException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.LimitExceededException("Too many sign in attempts. Please try again later", ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidCredentialsException($"Sign in failed: {ex.Message}", ex);
        }
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ForgotPasswordRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
                SecretHash = GenerateSecretHash(email)
            };

            await _cognitoClient.ForgotPasswordAsync(request, cancellationToken);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.UserNotFoundException(email, ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.LimitExceededException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.LimitExceededException("Too many requests. Please try again later", ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new PasswordResetFailedException($"Failed to initiate password reset: {ex.Message}", ex);
        }
    }

    public async Task ResetPasswordAsync(string email, string resetCode, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmForgotPasswordRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
                ConfirmationCode = resetCode,
                Password = newPassword,
                SecretHash = GenerateSecretHash(email)
            };

            await _cognitoClient.ConfirmForgotPasswordAsync(request, cancellationToken);
        }
        catch (CodeMismatchException ex)
        {
            throw new InvalidConfirmationCodeException("Invalid reset code", ex);
        }
        catch (ExpiredCodeException ex)
        {
            throw new InvalidConfirmationCodeException("Reset code has expired", ex);
        }
        catch (CognitoInvalidPasswordException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.InvalidPasswordException("Password does not meet security requirements", ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.UserNotFoundException(email, ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new PasswordResetFailedException($"Failed to reset password: {ex.Message}", ex);
        }
    }

    public async Task ChangePasswordAsync(string accessToken, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ChangePasswordRequest
            {
                AccessToken = accessToken,
                PreviousPassword = currentPassword,
                ProposedPassword = newPassword
            };

            await _cognitoClient.ChangePasswordAsync(request, cancellationToken);
        }
        catch (NotAuthorizedException ex)
        {
            throw new InvalidCredentialsException("Invalid current password", ex);
        }
        catch (CognitoInvalidPasswordException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.InvalidPasswordException("New password does not meet security requirements", ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.LimitExceededException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.LimitExceededException("Too many requests. Please try again later", ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new PasswordResetFailedException($"Failed to change password: {ex.Message}", ex);
        }
    }

    public async Task EnableUserAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AdminEnableUserRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = email
            };

            await _cognitoClient.AdminEnableUserAsync(request, cancellationToken);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.UserNotFoundException(email, ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidOperationException($"Failed to enable user: {ex.Message}", ex);
        }
    }

    public async Task DisableUserAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AdminDisableUserRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = email
            };

            await _cognitoClient.AdminDisableUserAsync(request, cancellationToken);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.UserNotFoundException(email, ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidOperationException($"Failed to disable user: {ex.Message}", ex);
        }
    }

    #region private methods

    private string GenerateSecretHash(string username)
    {
        var message = username + _settings.ClientId;
        var keyBytes = Encoding.UTF8.GetBytes(_settings.ClientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }

    private static Token TokenFromAuthResult(AuthenticationResultType authResult, string? refreshToken = null)
    {
        return new Token(
            AccessToken: authResult.AccessToken,
            IdToken: authResult.IdToken,
            RefreshToken: refreshToken ?? authResult.RefreshToken,
            ExpiresIn: authResult.ExpiresIn
        );
    }

    private async Task AddUserToGroupAsync(User user, CancellationToken cancellationToken, bool createGroupIfNotExists = true)
    {
        try
        {
            var request = new AdminAddUserToGroupRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = user.Email,
                GroupName = user.Role.Name
            };

            await _cognitoClient.AdminAddUserToGroupAsync(request, cancellationToken);
        }
        catch (ResourceNotFoundException ex)
        {
            throw new InvalidOperationException($"Group '{user.Role.Name}' does not exist in the user pool", ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            throw new FCG.Lib.Shared.Application.Common.Exceptions.UserNotFoundException(user.Email, ex);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidOperationException($"Failed to add user to group '{user.Role.Name}': {ex.Message}", ex);
        }
    }

    #endregion
}
