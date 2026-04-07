using StrategyPattern.Api.Domain.Constants;

namespace StrategyPattern.Api.Application.Abstractions.Providers;

public interface IUserProvider
{
    Profile GetActiveProfile();
}