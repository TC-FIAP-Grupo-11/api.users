using MediatR;
using FluentValidation;
using FCG.Lib.Shared.Application.Common.Models;

namespace FCG.Api.Users.Application.Commands.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
