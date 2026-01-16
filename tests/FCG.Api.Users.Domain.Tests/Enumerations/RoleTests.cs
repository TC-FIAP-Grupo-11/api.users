using FCG.Lib.Shared.Domain.Enumerations;
using FluentAssertions;

namespace FCG.Api.Users.Domain.Tests.Enumerations;

public class RoleTests
{
    [Theory]
    [InlineData("Admin")]
    [InlineData("admin")]
    [InlineData("ADMIN")]
    public void GivenValidAdminName_WhenGettingByName_ThenShouldReturnAdminRole(string name)
    {
        // Given (Dado)
        var role = Role.FromName(name);

        // When & Then (Quando & Então)
        role.Should().Be(Role.Admin);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("SuperAdmin")]
    public void GivenInvalidName_WhenGettingByName_ThenShouldThrowArgumentException(string invalidName)
    {
        // Given (Dado)
        var act = () => Role.FromName(invalidName);

        // When & Then (Quando & Então)
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid role name: {invalidName}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void GivenInvalidId_WhenGettingById_ThenShouldThrowArgumentException(int invalidId)
    {
        // Given (Dado)
        var act = () => Role.FromId(invalidId);

        // When & Then (Quando & Então)
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid role id: {invalidId}");
    }
}
