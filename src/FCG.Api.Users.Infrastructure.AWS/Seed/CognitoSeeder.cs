using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using FCG.Api.Users.Domain.Entities;
using FCG.Api.Users.Domain.Enumerations;
using FCG.Api.Users.Infrastructure.AWS.Cognito.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Api.Users.Infrastructure.AWS.Seed;

public class CognitoSeeder(
    IAmazonCognitoIdentityProvider cognitoClient,
    IOptions<CognitoSettings> settings,
    ILogger<CognitoSeeder> logger)
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient = cognitoClient;
    private readonly CognitoSettings _settings = settings.Value;
    private readonly ILogger<CognitoSeeder> _logger = logger;

    public async Task<string> SeedAdminAsync(User adminUser, string adminPassword, CancellationToken cancellationToken = default)
    {
        await CreateGroupsFromRolesAsync(cancellationToken);

        var adminId = await CreateAdminUserAsync(adminUser, adminPassword, cancellationToken);

        return adminId;
    }

    private async Task CreateGroupsFromRolesAsync(CancellationToken cancellationToken)
    {
        var listGroupsRequest = new ListGroupsRequest
        {
            UserPoolId = _settings.UserPoolId
        };
        var listGroupsResponse = await _cognitoClient.ListGroupsAsync(listGroupsRequest, cancellationToken);
        var existingGroupNames = listGroupsResponse.Groups.Select(g => g.GroupName).ToHashSet();

        foreach (var role in Role.List())
        {
            if (existingGroupNames.Contains(role.Name))
                continue;

            var request = new CreateGroupRequest
            {
                UserPoolId = _settings.UserPoolId,
                GroupName = role.Name,
                Description = $"Group for role: {role.Name}"
            };

            await _cognitoClient.CreateGroupAsync(request, cancellationToken);
            _logger.LogInformation("Group {GroupName} created", role.Name);
        }
    }

    private async Task<string> CreateAdminUserAsync(User adminUser, string adminPassword, CancellationToken cancellationToken)
    {
        try
        {
            var getUserRequest = new AdminGetUserRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = adminUser.Email
            };

            var existingUser = await _cognitoClient.AdminGetUserAsync(getUserRequest, cancellationToken);
            _logger.LogInformation("Admin user {Email} already exists", adminUser.Email);
            return existingUser.Username;
        }
        catch (UserNotFoundException)
        {
            var createUserRequest = new AdminCreateUserRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = adminUser.Email,
                TemporaryPassword = adminPassword,
                UserAttributes =
                [
                    new() { Name = "email", Value = adminUser.Email },
                    new() { Name = "name", Value = adminUser.Name },
                    new() { Name = "email_verified", Value = "true" }
                ],
                MessageAction = MessageActionType.SUPPRESS
            };

            var createResponse = await _cognitoClient.AdminCreateUserAsync(createUserRequest, cancellationToken);

            var addToGroupRequest = new AdminAddUserToGroupRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = adminUser.Email,
                GroupName = Role.Admin.Name
            };
            await _cognitoClient.AdminAddUserToGroupAsync(addToGroupRequest, cancellationToken);

            var setPasswordRequest = new AdminSetUserPasswordRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = adminUser.Email,
                Password = adminPassword,
                Permanent = true
            };
            await _cognitoClient.AdminSetUserPasswordAsync(setPasswordRequest, cancellationToken);

            _logger.LogInformation("Admin user {Email} created successfully", adminUser.Email);
            return createResponse.User.Username;
        }
    }
}
