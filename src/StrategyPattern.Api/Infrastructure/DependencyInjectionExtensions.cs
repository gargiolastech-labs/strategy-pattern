using System.Reflection;
using StrategyPattern.Api.Application.Abstractions.Providers;
using StrategyPattern.Api.Application.Abstractions.Strategies;
using StrategyPattern.Api.Application.Features.Users.Commands.AddUser;
using StrategyPattern.Api.Application.Features.Users.Commands.AddUser.Strategies;
using StrategyPattern.Api.Domain.Users;
using StrategyPattern.Api.Infrastructure.Providers;
using StrategyPattern.Api.Infrastructure.Strategies;

namespace StrategyPattern.Api.Infrastructure;

internal static class DependencyInjectionExtensions
{
    internal static void InitializeInfrastructre(this IServiceCollection services, IConfiguration configuration)
    {
        InitializeMediatr(services, typeof(DependencyInjectionExtensions).Assembly);
        InitializeProviders(services);
        InitializeAddUserCommandStrategy(services);
        
        services.AddScoped(typeof(IStrategySelector<,>), typeof(StrategySelector<,>));
    }

    private static void InitializeMediatr(IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(config => { config.RegisterServicesFromAssembly(assembly); });
    }

    private static void InitializeProviders(IServiceCollection services)
    {
        services.AddScoped<IUserProvider, UserProvider>();
    }

    private static void InitializeAddUserCommandStrategy(IServiceCollection services)
    {
        services.AddScoped<IHandlerStrategy<AddUserContext, User>, AddUserAdminStrategy>();
        services.AddScoped<IHandlerStrategy<AddUserContext, User>, AddUserCustomStrategy>();
        services.AddScoped<IHandlerStrategy<AddUserContext, User>, AddUserGuestStrategy>();
    }
    
}