namespace FCG.Api.Users.Domain.Enumerations;

public sealed class Role
{
    public static readonly Role Admin = new(1, "Admin");
    public static readonly Role User = new(2, "User");

    public int Id { get; }
    public string Name { get; }

    private Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public static IEnumerable<Role> List() => [Admin, User];

    public static Role FromName(string name) =>
        List().FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Invalid role name: {name}");

    public static Role FromId(int id) =>
        List().FirstOrDefault(r => r.Id == id)
        ?? throw new ArgumentException($"Invalid role id: {id}");
}
