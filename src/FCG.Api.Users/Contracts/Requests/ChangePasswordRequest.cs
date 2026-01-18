namespace FCG.Api.Users.Contracts.Requests;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
