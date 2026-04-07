using CSharpFunctionalExtensions;

namespace StrategyPattern.Api.Application.Abstractions.Strategies;

public interface IHandlerStrategy<in TContext, TResponse> 
    where TContext : IContext
{
    Task<bool> CanHandleAsync(TContext context, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}