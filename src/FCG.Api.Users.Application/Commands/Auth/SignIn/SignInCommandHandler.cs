using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Api.Users.Application.Contracts.Auth.Responses;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;

namespace FCG.Api.Users.Application.Commands.Auth.SignIn;

public class SignInCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<SignInCommand, Result<Token>>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result<Token>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authenticationService.SignInAsync(
                request.Email,
                request.Password,
                cancellationToken
            );

            return Result.Success(token);
        }
        catch (InvalidCredentialsException)
        {
            return Result.Failure<Token>(ApplicationErrors.Authentication.InvalidCredentials);
        }
        catch (EmailNotConfirmedException)
        {
            return Result.Failure<Token>(ApplicationErrors.Authentication.EmailNotConfirmed);
        }
        catch (UserDisabledException)
        {
            return Result.Failure<Token>(ApplicationErrors.Authentication.UserDisabled);
        }
        catch (UserNotFoundException)
        {
            return Result.Failure<Token>(ApplicationErrors.Authentication.InvalidCredentials);
        }
        catch (LimitExceededException ex)
        {
            return Result.Failure<Token>(Error.Failure("SignIn.TooManyAttempts", ex.Message));
        }
        catch (AuthenticationException ex)
        {
            return Result.Failure<Token>(
                Error.Failure("SignIn.Failed", $"Failed to sign in: {ex.Message}"));
        }
    }
}
