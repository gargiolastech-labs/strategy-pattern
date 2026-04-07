using StrategyPattern.Api.Application.Abstractions.Strategies;

namespace StrategyPattern.Api.Infrastructure.Strategies;

internal sealed class StrategySelector<TContext, TResponse> : IStrategySelector<TContext, TResponse>
    where TContext : IContext
{
    private readonly IEnumerable<IHandlerStrategy<TContext, TResponse>> _strategies;

    public StrategySelector(IEnumerable<IHandlerStrategy<TContext, TResponse>> strategies)
    {
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    }

    public async Task<IHandlerStrategy<TContext, TResponse>> SelectAsync(TContext context,
        CancellationToken cancellationToken = default)
    {
        var activeStrategies = new List<IHandlerStrategy<TContext, TResponse>>();

        foreach (var handlerStrategy in _strategies)
        {
            if (await handlerStrategy.CanHandleAsync(context, cancellationToken))
            {
                activeStrategies.Add(handlerStrategy);
            }
        }

        return activeStrategies.Count is 0 or > 1 ?
            throw new InvalidOperationException("Deve essere presente una strategia attiva.") : 
          activeStrategies.First();
    }
}