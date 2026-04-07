using CSharpFunctionalExtensions;
using MediatR;

namespace StrategyPattern.Api.Application.Abstractions.Messages;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCQRS;