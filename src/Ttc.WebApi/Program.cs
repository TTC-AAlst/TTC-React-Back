using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using Serilog;
using Serilog.Context;
using System.Text.Json.Serialization;
using Ttc.DataAccess;
using Ttc.DataEntities.Core;
using Ttc.Model.Core;
using Ttc.WebApi.Emailing;
using Ttc.WebApi.Utilities;
using Ttc.WebApi.Utilities.Auth;
using Ttc.WebApi.Utilities.Pipeline;
using Ttc.WebApi.Utilities.PongRank;

try
{
    var (ttcSettings, configuration) = LoadSettings.GetConfiguration();

    SetupLogger.Configure(ttcSettings);
    Log.Information("Starting up...");

    ExcelPackage.License.SetNonCommercialOrganization("TTC Aalst");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSingleton(ttcSettings);
    builder.Services.AddSingleton(ttcSettings.Email);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", corsBuilder =>
        {
            corsBuilder
                .WithOrigins(ttcSettings.Origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
    builder.Services.AddSerilog(Log.Logger);
    builder.Services.AddSingleton<TtcLogger>();
    builder.Services.AddScoped<EmailService>();
    builder.Services.AddScoped<UserProvider>();
    builder.Services.AddScoped<IUserProvider>(sp => sp.GetRequiredService<UserProvider>());
    builder.Services.AddScoped<PongRankClient>();
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpClient();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddControllers().AddControllersAsServices().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.WriteIndented = false;
    });
    builder.Services.AddEndpointsApiExplorer();
    AddSwagger.Configure(builder.Services);
    GlobalBackendConfiguration.Configure(builder.Services, configuration);
    AddAuthentication.Configure(builder.Services, ttcSettings);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddSignalR();
    if (ttcSettings.StartSyncJob)
    {
        builder.Services.AddHostedService<FrenoySyncJob>();
    }

    builder.Services.AddServiceModelServices().AddServiceModelMetadata();
    builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

    var app = builder.Build();
    app.UseSwagger();
    app.UseSwaggerUI();
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Starting Development Server");
        app.UseDeveloperExceptionPage();
    }
    else
    {
        Log.Information("Starting Release Server");
        // app.UseHsts();
        // app.UseHttpsRedirection();
    }

    var serviceMetadataBehavior = app.Services.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;

    app.UseCors("CorsPolicy");

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(ttcSettings.PublicImageFolder),
        RequestPath = "/img"
    });

    app.UseAuthentication();
    app.UseAuthorization();
    app.Use(async (context, next) =>
    {
        LogContext.PushProperty("UserName", context.User.Identity?.Name ?? "Anonymous");
        LogContext.PushProperty("env", app.Environment.EnvironmentName);
        await next();
    });
    app.UseMiddleware<RequestLoggingFilter>();
    //app.UseSerilogRequestLogging(options =>
    //{
    //    options.GetLevel = (httpContext, elapsed, exception) =>
    //        LogEventLevel.Warning;
    //});
    app.MapControllers();
    app.UseExceptionHandler();
    app.MapHub<TtcHub>("/hubs/ttc");
    app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ITtcDbContext>();
        dbContext.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Something went wrong");
}
finally
{
    await Log.CloseAndFlushAsync();
}
