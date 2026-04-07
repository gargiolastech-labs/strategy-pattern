using CSharpFunctionalExtensions;
using MediatR;

namespace StrategyPattern.Api.Application.Abstractions.Messages;

public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : ICommand<TResponse>;