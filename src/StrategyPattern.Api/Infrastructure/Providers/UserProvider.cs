using System.Security.Cryptography;
using StrategyPattern.Api.Application.Abstractions.Providers;
using StrategyPattern.Api.Domain.Constants;

namespace StrategyPattern.Api.Infrastructure.Providers;

internal sealed class UserProvider : IUserProvider
{
    public Profile GetActiveProfile()
    {
       var number = RandomNumberGenerator.GetInt32(1, 3);
       return Enum.Parse<Profile>(number.ToString());
    }
}