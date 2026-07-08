using ManteqTask.Application.Services;
using ManteqTask.Application.Utilities;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Infrastructure.Persistence;
using ManteqTask.Infrastructure.Persistence.Repositories;
using ManteqTask.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManteqTask.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetRequiredSetting("ConnectionStrings:DbConnectionString");

        services.AddDbContext<ApplicationDbContext>((IServiceProvider provider, DbContextOptionsBuilder options) =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());
        // Add your repositories like this here
        // services.AddScoped<IRepository, Repository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IRequestNumberGenerator, RequestNumberGenerator>();
        services.AddLogging();

        return services;
    }
}
