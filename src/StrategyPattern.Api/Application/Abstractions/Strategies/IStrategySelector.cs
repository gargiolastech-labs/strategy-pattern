namespace StrategyPattern.Api.Application.Abstractions.Strategies;

public interface IStrategySelector<TContext, TResponse> where TContext : IContext
{
    Task<IHandlerStrategy<TContext, TResponse>> SelectAsync(TContext context, CancellationToken cancellationToken = default);
}