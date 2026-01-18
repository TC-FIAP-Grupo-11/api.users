using FCG.Api.Users.Application.Contracts.Auth.Responses;
using FCG.Api.Users.Domain.Entities;

namespace FCG.Api.Users.Application.Contracts.Auth;

public interface IAuthenticationService
{
    Task<string> SignUpAsync(User user, string password, CancellationToken cancellationToken = default);
    Task ConfirmSignUpAsync(string email, string confirmationCode, CancellationToken cancellationToken = default);
    Task ResendConfirmationCodeAsync(string email, CancellationToken cancellationToken = default);
    Task<Token> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string email, string resetCode, string newPassword, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(string accessToken, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task EnableUserAsync(string email, CancellationToken cancellationToken = default);
    Task DisableUserAsync(string email, CancellationToken cancellationToken = default);
}
