using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;

namespace FCG.Api.Users.Application.Commands.Auth.DisableUser;

public class DisableUserCommandHandler(IAuthenticationService authenticationService) : IRequestHandler<DisableUserCommand, Result>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result> Handle(DisableUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.DisableUserAsync(
                request.Email,
                cancellationToken
            );

            return Result.Success();
        }
        catch (UserNotFoundException)
        {
            return Result.Failure(ApplicationErrors.Authentication.UserNotFound);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("DisableUser.Failed", ex.Message));
        }
    }
}
