using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Api.Users.Domain.Entities;
using FCG.Api.Users.Application.Contracts.Repositories;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;
using FCG.Lib.Shared.Messaging.Contracts;
using MassTransit;

namespace FCG.Api.Users.Application.Commands.Auth.SignUp;

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IPublishEndpoint _publishEndpoint;

    public SignUpCommandHandler(
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _publishEndpoint = publishEndpoint;
    }

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

            // Publicar evento de usuário criado
            await _publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                CreatedAt = user.CreatedAt
            }, cancellationToken);

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