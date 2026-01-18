using FCG.Lib.Shared.Application.Common.Exceptions;
using MediatR;
using FCG.Api.Users.Application.Contracts.Auth;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Lib.Shared.Application.Common.Errors;
using FCG.Lib.Shared.Messaging.Contracts;
using MassTransit;
using FCG.Api.Users.Application.Contracts.Repositories;

namespace FCG.Api.Users.Application.Commands.Auth.ConfirmSignUp;

public class ConfirmSignUpCommandHandler(
    IAuthenticationService authenticationService,
    IPublishEndpoint publishEndpoint,
    IUserRepository userRepository) : IRequestHandler<ConfirmSignUpCommand, Result>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result> Handle(ConfirmSignUpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null)
            {
                return Result.Failure(ApplicationErrors.User.NotFound(request.Email));
            }
            
            await _authenticationService.ConfirmSignUpAsync(
                request.Email,
                request.ConfirmationCode,
                cancellationToken
            );

            // Publicar evento de usuário criado
            await _publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                CreatedAt = user.CreatedAt
            }, cancellationToken);

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
