using Microsoft.AspNetCore.Http;

namespace Ttc.DataEntities.Core;

public interface IUserNameProvider
{
    string Name { get; }
}

/// <summary>
/// UserName from JWT token
/// </summary>
public class WebUserNameProvider : IUserNameProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public string Name => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";

    public WebUserNameProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
}

/// <summary>
/// DbMigrations UserName
/// </summary>
public class MigrationsUserNameProvider : IUserNameProvider
{
    public string Name => "Migrations";
}