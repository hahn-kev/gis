using Backend.Services;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Identity;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend.DataLayer;

public class DbStartupService : IHostedService
{
    private readonly ILogger<DbStartupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DbStartupService(ILogger<DbStartupService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        var dbConnection = serviceScope.ServiceProvider.GetRequiredService<IDbConnection>();
        var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userService = serviceScope.ServiceProvider.GetRequiredService<UserService>();
        
        DataConnection.TurnTraceSwitchOn();
        DataConnection.WriteTraceLine = (message, category) => _logger.LogDebug(message);
        LinqToDB.Common.Configuration.Linq.AllowMultipleQuery = true;
        DbConnection.SetupMappingBuilder(MappingSchema.Default);
#if DEBUG
        var missingRoles =
            new[] { "admin", "hr", "hradmin", "registrar" }.Except(roleManager.Roles.Select(role => role.Name));
        foreach (var missingRole in missingRoles)
        {
            await roleManager.CreateAsync(new IdentityRole<int>(missingRole){ConcurrencyStamp = Guid.NewGuid().ToString("N")});
        }

        if (!dbConnection.Users.Any())
        {
            var identityUser = new IdentityUser
            {
                UserName = "khahn",
                ResetPassword = true
            };
            await userService.CreateAsync(identityUser, "password");
            await userService.AddToRoleAsync(identityUser, "admin");
        }
#endif
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}