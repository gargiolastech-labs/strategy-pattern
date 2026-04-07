using CSharpFunctionalExtensions;
using MediatR;

namespace StrategyPattern.Api.Application.Abstractions.Messages;

public interface IQuery<TQuery> : IRequest<Result<TQuery>>, IBaseCQRS;