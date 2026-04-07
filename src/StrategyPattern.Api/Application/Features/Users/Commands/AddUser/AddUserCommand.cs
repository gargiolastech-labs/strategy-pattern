using StrategyPattern.Api.Application.Abstractions.Messages;

namespace StrategyPattern.Api.Application.Features.Users.Commands.AddUser;

public sealed record AddUserCommand(string Name, string LastName) : ICommand<Guid?>;