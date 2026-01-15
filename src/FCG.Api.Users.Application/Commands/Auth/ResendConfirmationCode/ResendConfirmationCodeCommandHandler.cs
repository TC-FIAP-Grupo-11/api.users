using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;

namespace FCG.Api.Users.Application.Commands.Auth.ResendConfirmationCode;

public class ResendConfirmationCodeCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<ResendConfirmationCodeCommand, Result>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result> Handle(ResendConfirmationCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.ResendConfirmationCodeAsync(
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
            return Result.Failure(
                Error.Failure("ResendCode.LimitExceeded", ex.Message));
        }
        catch (InvalidConfirmationCodeException ex)
        {
            return Result.Failure(
                Error.Failure("ResendCode.Failed", ex.Message));
        }
        catch (AuthenticationException ex)
        {
            return Result.Failure(
                Error.Failure("ResendCode.Failed", $"Failed to resend confirmation code: {ex.Message}"));
        }
    }
}
