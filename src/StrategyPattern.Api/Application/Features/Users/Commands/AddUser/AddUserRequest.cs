namespace StrategyPattern.Api.Application.Features.Users.Commands.AddUser;

public sealed class AddUserRequest
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
}