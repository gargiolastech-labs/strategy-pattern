using StrategyPattern.Api.Domain.Constants;

namespace StrategyPattern.Api.Application.Abstractions.Strategies;

public interface IContext
{
    Profile Profile { get; }
}