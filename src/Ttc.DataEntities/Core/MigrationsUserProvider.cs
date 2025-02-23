namespace Ttc.DataEntities.Core;

/// <summary>
/// DbMigrations UserName
/// </summary>
public class MigrationsUserProvider : IUserProvider
{
    public string Name => "Migrations";
    public int? PlayerId => null;
}
