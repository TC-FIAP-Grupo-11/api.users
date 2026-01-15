using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;

namespace FCG.Api.Users.Application.Commands.Auth.ResetPassword;

public class ResetPasswordCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.ResetPasswordAsync(
                request.Email,
                request.ResetCode,
                request.NewPassword,
                cancellationToken
            );

            return Result.Success();
        }
        catch (InvalidConfirmationCodeException)
        {
            return Result.Failure(ApplicationErrors.Authentication.InvalidConfirmationCode);
        }
        catch (InvalidPasswordException)
        {
            return Result.Failure(ApplicationErrors.Authentication.WeakPassword);
        }
        catch (UserNotFoundException)
        {
            return Result.Failure(ApplicationErrors.Authentication.UserNotFound);
        }
        catch (PasswordResetFailedException)
        {
            return Result.Failure(ApplicationErrors.Authentication.PasswordResetFailed);
        }
        catch (AuthenticationException ex)
        {
            return Result.Failure(
                Error.Failure("ResetPassword.Failed", $"Failed to reset password: {ex.Message}"));
        }
    }
}
