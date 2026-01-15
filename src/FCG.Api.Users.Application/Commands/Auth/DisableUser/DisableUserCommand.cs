using MediatR;
using FluentValidation;
using FCG.Lib.Shared.Application.Common.Models;

namespace FCG.Api.Users.Application.Commands.Auth.DisableUser;

public record DisableUserCommand(string Email) : IRequest<Result>;

public class DisableUserCommandValidator : AbstractValidator<DisableUserCommand>
{
    public DisableUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
