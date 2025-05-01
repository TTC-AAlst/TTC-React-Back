using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ttc.Model.Core;

namespace Ttc.WebApi.Utilities.Auth;

internal static class AddAuthentication
{
    public static void Configure(IServiceCollection services, TtcSettings settings)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = UserProvider.CreateTokenParameters(settings);
            });

        services.AddAuthorization();
    }
}