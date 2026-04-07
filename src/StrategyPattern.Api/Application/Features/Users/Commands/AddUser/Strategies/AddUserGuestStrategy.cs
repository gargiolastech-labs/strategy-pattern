using CSharpFunctionalExtensions;
using StrategyPattern.Api.Application.Abstractions.Strategies;
using StrategyPattern.Api.Domain.Constants;
using StrategyPattern.Api.Domain.Users;

namespace StrategyPattern.Api.Application.Features.Users.Commands.AddUser.Strategies;

public class AddUserGuestStrategy : IHandlerStrategy<AddUserContext, User>
{
    private const Profile Profile = Domain.Constants.Profile.Guest;
    
    public async Task<bool> CanHandleAsync(AddUserContext context, CancellationToken cancellationToken = default)
    {
        return context.Profile ==  Profile;
    }

    public async Task<Result<User>> ExecuteAsync(AddUserContext context, CancellationToken cancellationToken = default)
    {
        var user = User.Create(context.Command.Name, context.Command.LastName);
        user.SetProfileTypeId(Profile);
        
        return Result.Success(user);
    }
}