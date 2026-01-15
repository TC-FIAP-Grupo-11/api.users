namespace FCG.Api.Users.Contracts.Responses;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string? AccountId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
