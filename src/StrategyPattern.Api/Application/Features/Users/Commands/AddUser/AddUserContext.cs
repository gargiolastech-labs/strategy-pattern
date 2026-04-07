using StrategyPattern.Api.Application.Abstractions.Strategies;
using StrategyPattern.Api.Domain.Constants;

namespace StrategyPattern.Api.Application.Features.Users.Commands.AddUser;

public sealed class AddUserContext : IContext
{
    private AddUserContext(Profile profile, AddUserCommand command)
    {
        Profile = profile;
        Command = command;
    }

    public Profile Profile { get; private set; }

    public AddUserCommand Command { get; private set; }

    internal static AddUserContext CreateInstance(Profile profile, AddUserCommand command)
    {
        AddUserContext context = new(profile, command);
        return context;
    }
}