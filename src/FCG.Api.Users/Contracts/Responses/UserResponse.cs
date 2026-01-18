namespace FCG.Api.Users.Contracts.Responses;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    RoleDto Role,
    string? AccountId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record RoleDto(
    int Id,
    string Name
);
