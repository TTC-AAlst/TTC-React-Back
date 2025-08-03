using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using Ttc.Model.Core;

namespace Ttc.WebApi.Utilities.Pipeline;

public static class SetupLogger
{
    public static void Configure(TtcSettings ttcSettings)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)

            // These two don't seem to be turning anything off?
            //.MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            //.MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)

            // Turn off HTTP GET/POST logs:
            //.MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)

            // Turn everything off:
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)

            .Enrich.WithMachineName()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {Properties}{NewLine}{Exception}")
            .WriteTo.File(
                "logs/log.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {Properties}{NewLine}{Exception}",
                shared: true
            )
            .WriteTo.GrafanaLoki(
                ttcSettings.Loki,
                [
                    new LokiLabel() { Key = "service_name", Value = "ttc-backend" },
                    new LokiLabel() { Key = "app", Value = "ttc" },
                ],
                [
                    "level",
                    "MachineName",
                    "UserName",
                    "RequestId",
                    "app",
                    "env"
                ])
            .CreateLogger();
    }
}