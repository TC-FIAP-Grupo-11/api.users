using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Api.Users.Domain.Entities;
using FCG.Api.Users.Application.Contracts.Repositories;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;

namespace FCG.Api.Users.Application.Commands.Auth.SignUp;

public class SignUpCommandHandler(IUserRepository userRepository, IAuthenticationService authenticationService) : IRequestHandler<SignUpCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<Result<Guid>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
                return Result.Failure<Guid>(ApplicationErrors.User.EmailAlreadyExists(request.Email));

            var user = User.CreateUser(
                request.Name,
                request.Email
            );

            var cognitoUserId = await _authenticationService.SignUpAsync(
                user,
                request.Password,
                cancellationToken
            );

            user.SetAccountId(cognitoUserId);

            await _userRepository.AddAsync(user);

            return Result.Success(user.Id);
        }
        catch (UserAlreadyExistsException)
        {
            return Result.Failure<Guid>(ApplicationErrors.User.EmailAlreadyExists(request.Email));
        }
        catch (InvalidPasswordException ex)
        {
            return Result.Failure<Guid>(Error.Validation("SignUp.InvalidPassword", ex.Message));
        }
        catch (LimitExceededException ex)
        {
            return Result.Failure<Guid>(Error.Failure("SignUp.TooManyAttempts", ex.Message));
        }
        catch (AuthenticationException ex)
        {
            return Result.Failure<Guid>(
                Error.Failure("SignUp.Failed", $"Failed to sign up user: {ex.Message}"));
        }
    }
}