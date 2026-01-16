using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FCG.Api.Users.Application.Commands.Auth.SignUp;
using FCG.Api.Users.Application.Commands.Auth.ConfirmSignUp;
using FCG.Api.Users.Application.Commands.Auth.ResendConfirmationCode;
using FCG.Api.Users.Application.Commands.Auth.SignIn;
using FCG.Api.Users.Application.Commands.Auth.ForgotPassword;
using FCG.Api.Users.Application.Commands.Auth.ResetPassword;
using FCG.Api.Users.Application.Commands.Auth.ChangePassword;
using FCG.Api.Users.Application.Commands.Auth.EnableUser;
using FCG.Api.Users.Application.Commands.Auth.DisableUser;
using FCG.Lib.Shared.Application.Extensions;
using FCG.Lib.Shared.Application.Common.Models;
using FCG.Api.Users.Contracts.Responses;
using FCG.Api.Users.Application.Queries.Users.GetUserByEmail;

namespace FCG.Api.Users.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("{email}")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await _mediator.Send(query);
        
        return result.ToActionResult();
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SignUpResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return result.ToActionResult();

        return CreatedAtAction(
            nameof(SignUp),
            new { id = result.Value },
            new { userId = result.Value, message = "User registered successfully. Please check your email to confirm your account." });
    }

    [HttpPost("confirm-signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmSignUp([FromBody] ConfirmSignUpCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(new { message = "Email confirmed successfully. You can now sign in." });
    }

    [HttpPost("resend-confirmation-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendConfirmationCode([FromBody] ResendConfirmationCodeCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(new { message = "Confirmation code resent successfully. Please check your email." });
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SignInResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] SignInCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(new { message = "Password reset code sent to your email." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(new { message = "Password reset successfully. You can now sign in with your new password." });
    }

    [HttpPost("change-password")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return result.ToActionResult();

        return Ok(new { message = "Password changed successfully." });
    }

    [HttpPatch("{email}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(string email, [FromQuery] bool enabled)
    {
        var result = enabled
            ? await _mediator.Send(new EnableUserCommand(email))
            : await _mediator.Send(new DisableUserCommand(email));

        if (result.IsFailure)
            return result.ToActionResult();

        var message = enabled ? "User enabled successfully." : "User disabled successfully.";
        return Ok(new { message });
    }
}
