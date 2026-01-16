using FCG.Lib.Shared.Domain.Entities;
using FCG.Lib.Shared.Domain.Enumerations;

namespace FCG.Api.Users.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public Role Role { get; private set; }
    public string? AccountId { get; private set; }

    private User(string name, string email, Role role)
    {
        Name = name;
        Email = email;
        Role = role;
    }

    public static User CreateUser(string name, string email)
    {
        return Create(name, email, Role.User);
    }

    public static User CreateAdmin(string name, string email)
    {
        return Create(name, email, Role.Admin);
    }

    public void SetAccountId(string accountId)
        => AccountId = accountId;

    private static User Create(string name, string email, Role role)
    {
        ValidateName(name);
        ValidateEmail(email);
        
        return new(name, email, role);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
    }
    
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        
        if (!email.Contains("@"))
            throw new ArgumentException("Invalid email format.", nameof(email));
    }
}
