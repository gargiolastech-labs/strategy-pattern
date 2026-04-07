using CSharpFunctionalExtensions;
using StrategyPattern.Api.Application.Abstractions.Messages;
using StrategyPattern.Api.Application.Abstractions.Providers;
using StrategyPattern.Api.Application.Abstractions.Strategies;
using StrategyPattern.Api.Domain.Users;

namespace StrategyPattern.Api.Application.Features.Users.Commands.AddUser;

public sealed class AddUserCommandHandler : ICommandHandler<AddUserCommand, Guid?>
{
    private readonly IUserProvider _userProvider;
    private readonly IStrategySelector<AddUserContext, User> _strategySelector;

    public AddUserCommandHandler(IUserProvider userProvider, IStrategySelector<AddUserContext, User> strategySelector)
    {
        _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
    }

    public async Task<Result<Guid?>> Handle(AddUserCommand request, CancellationToken cancellationToken)
    {
        return await AddUserAsync(request, cancellationToken);
    }

    private async Task<Result<Guid?>> AddUserAsync(AddUserCommand request, CancellationToken cancellationToken = default)
    {
        var profile = _userProvider.GetActiveProfile();
        var context = AddUserContext.CreateInstance(profile, request);

        var activeStrategy = await _strategySelector.SelectAsync(context, cancellationToken);
        var result = await activeStrategy.ExecuteAsync(context, cancellationToken);

        if (result.IsSuccess)
            return Result.Success<Guid?>(result.Value.Id);
        
        return result.ConvertFailure<Guid?>();
    }
}