using ClaudeProjectBackend.Application.Auth;
using ClaudeProjectBackend.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeProjectBackend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
