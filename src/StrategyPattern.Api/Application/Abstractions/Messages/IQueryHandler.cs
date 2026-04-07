using CSharpFunctionalExtensions;
using MediatR;

namespace StrategyPattern.Api.Application.Abstractions.Messages;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;