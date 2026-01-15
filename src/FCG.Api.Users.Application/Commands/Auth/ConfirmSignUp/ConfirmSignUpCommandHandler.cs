using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;

namespace FCG.Api.Users.Application.Commands.Auth.ConfirmSignUp;

public class ConfirmSignUpCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<ConfirmSignUpCommand, Result>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result> Handle(ConfirmSignUpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.ConfirmSignUpAsync(
                request.Email,
                request.ConfirmationCode,
                cancellationToken
            );

            return Result.Success();
        }
        catch (InvalidConfirmationCodeException)
        {
            return Result.Failure(ApplicationErrors.Authentication.InvalidConfirmationCode);
        }
        catch (UserNotFoundException)
        {
            return Result.Failure(ApplicationErrors.Authentication.UserNotFound);
        }
        catch (AuthenticationException ex)
        {
            return Result.Failure(
                Error.Failure("ConfirmSignUp.Failed", $"Failed to confirm sign up: {ex.Message}"));
        }
    }
}
