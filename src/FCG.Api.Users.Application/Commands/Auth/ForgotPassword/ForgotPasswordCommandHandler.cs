using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;

namespace FCG.Api.Users.Application.Commands.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.ForgotPasswordAsync(
                request.Email,
                cancellationToken
            );

            return Result.Success();
        }
        catch (UserNotFoundException)
        {
            return Result.Failure(ApplicationErrors.Authentication.UserNotFound);
        }
        catch (LimitExceededException ex)
        {
            return Result.Failure(Error.Failure("ForgotPassword.TooManyAttempts", ex.Message));
        }
        catch (PasswordResetFailedException ex)
        {
            return Result.Failure(
                Error.Failure("ForgotPassword.Failed", ex.Message));
        }
        catch (AuthenticationException ex)
        {
            return Result.Failure(
                Error.Failure("ForgotPassword.Failed", $"Failed to initiate password reset: {ex.Message}"));
        }
    }
}
