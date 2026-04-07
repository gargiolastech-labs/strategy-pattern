using StrategyPattern.Api.Domain.Constants;

namespace StrategyPattern.Api.Domain.Users;

public sealed class User : Entity
{
    private User(string name, string lastName, Guid userId) : base(userId)
    {
        Name = name;
        LastName = lastName;
    }

    public string Name { get; private set; }
    public string LastName { get; private set; }

    public Profile ProfileTypeId { get; private set; }

    internal static User Create(string name, string lastName)
    {
        User user = new User(name, lastName, Guid.CreateVersion7(DateTime.UtcNow));
        return user;
    }

    public void SetProfileTypeId(Profile profile)
    {
        ProfileTypeId = profile;
    }
}