using Backend.Entities;
using Backend.Services;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Identity;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend.DataLayer;

public class DbStartupService : IHostedService
{
    private readonly ILogger<DbStartupService> _logger;
    private readonly IDbConnection _dbConnection;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserService _userService;

    public DbStartupService(ILogger<DbStartupService> logger,
        IDbConnection dbConnection,
        RoleManager<IdentityRole<int>> roleManager,
        UserService userService)
    {
        _logger = logger;
        _dbConnection = dbConnection;
        _roleManager = roleManager;
        _userService = userService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        DataConnection.TurnTraceSwitchOn();
        DataConnection.WriteTraceLine = (message, category) => _logger.LogDebug(message);
        LinqToDB.Common.Configuration.Linq.AllowMultipleQuery = true;
        DbConnection.SetupMappingBuilder(MappingSchema.Default);
#if DEBUG
        var missingRoles =
            new[] { "admin", "hr", "hradmin", "registrar" }.Except(_roleManager.Roles.Select(role => role.Name));
        foreach (var missingRole in missingRoles)
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(missingRole));
        }

        if (!_dbConnection.Users.Any())
        {
            var identityUser = new IdentityUser
            {
                UserName = "khahn",
                ResetPassword = true
            };
            await _userService.CreateAsync(identityUser, "password");
            await _userService.AddToRoleAsync(identityUser, "admin");
        }
#endif
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}