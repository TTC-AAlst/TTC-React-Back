using Ttc.Model.Core;

namespace Ttc.WebApi.Utilities.Pipeline;

internal static class LoadSettings
{
    public static (TtcSettings, IConfigurationRoot) Configure(IServiceCollection services)
    {
        var ttcSettings = new TtcSettings();
        var configuration = new ConfigurationBuilder()
            // .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .Build();

        configuration
            .GetSection("TtcSettings")
            .Bind(ttcSettings);

        services.AddSingleton(ttcSettings);

        string? mailkitPassword = Environment.GetEnvironmentVariable("MAILKIT_PASSWORD");
        if (!string.IsNullOrWhiteSpace(mailkitPassword))
        {
            ttcSettings.Email.Password = ttcSettings.Email.Password.Replace("{MAILKIT_PASSWORD}", mailkitPassword);
        }
        services.AddSingleton(ttcSettings.Email);

        return (ttcSettings, configuration);
    }
}