using Ttc.Model.Core;

namespace Ttc.WebApi.Utilities;

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

        return (ttcSettings, configuration);
    }
}