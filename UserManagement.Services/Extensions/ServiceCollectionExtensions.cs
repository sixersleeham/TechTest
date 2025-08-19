using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    { 
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserLogService, UserLogService>();

        return services;
    }
}
