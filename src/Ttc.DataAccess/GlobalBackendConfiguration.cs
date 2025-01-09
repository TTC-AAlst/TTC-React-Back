using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities.AutoMapperConfig;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess;

/// <summary>
/// TTC Aalst DataAccess configuration
/// </summary>
public static class GlobalBackendConfiguration
{
    public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
    {
        ConfigureDbContext(services, configuration);
        ConfigureAutoMapper(services);
        ConfigureServices(services);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ClubService>();
        services.AddScoped<ConfigService>();
        services.AddScoped<MatchService>();
        services.AddScoped<TeamService>();
        services.AddScoped<PlayerService>();
    }

    private static void ConfigureAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(TeamProfile));
    }

    private static void ConfigureDbContext(IServiceCollection services, IConfigurationRoot configuration)
    {
        var connectionString = configuration.GetConnectionString("Ttc");
        services.AddDbContext<ITtcDbContext, TtcDbContext>(
            dbContextOptions => ConfigureDbContextBuilder(dbContextOptions, connectionString));
    }

    internal static void ConfigureDbContextBuilder(DbContextOptionsBuilder builder, string? connectionString = null)
    {
        // Replace with your server version and type.
        // Use 'MariaDbServerVersion' for MariaDB.
        // Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
        var serverVersion = new MySqlServerVersion(new Version(5, 5, 60));

        if (connectionString == null)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Ttc.WebApi"))
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();
            connectionString = configuration.GetConnectionString("Ttc") ?? "";
        }

        string? mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD");
        if (!string.IsNullOrWhiteSpace(mysqlPassword))
        {
            connectionString = connectionString.Replace("{MYSQL_ROOT_PASSWORD}", mysqlPassword);
        }

        builder.UseMySql(connectionString, serverVersion)
            // The following three options help with debugging, but should
            // be changed or removed for production.
            .LogTo(Console.WriteLine, LogLevel.Warning);
            //.EnableSensitiveDataLogging()
            //.EnableDetailedErrors()
    }
}
