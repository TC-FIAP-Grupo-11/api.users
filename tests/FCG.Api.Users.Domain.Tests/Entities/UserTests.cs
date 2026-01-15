using FCG.Api.Users.Domain.Entities;
using FluentAssertions;

namespace FCG.Api.Users.Domain.Tests.Entities;

public class UserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyName_WhenCreatingUser_ThenShouldThrowArgumentException(string invalidName)
    {
        // Given (Dado)
        var act = () => User.CreateUser(invalidName, "user@example.com");

        // When & Then (Quando & Então)
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be empty.*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyEmail_WhenCreatingUser_ThenShouldThrowArgumentException(string invalidEmail)
    {
        // Given (Dado)
        var act = () => User.CreateUser("John Doe", invalidEmail);

        // When & Then (Quando & Então)
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be empty.*");
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("invalid.email.com")]
    public void GivenInvalidEmailFormat_WhenCreatingUser_ThenShouldThrowArgumentException(string invalidEmail)
    {
        // Given (Dado)
        var act = () => User.CreateUser("John Doe", invalidEmail);

        // When & Then (Quando & Então)
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format.*");
    }
}
