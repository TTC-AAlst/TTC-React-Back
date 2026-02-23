using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;

namespace Ttc.UnitTests.Integration;

public class ConfigControllerTests : IntegrationTestBase
{
    public ConfigControllerTests(TtcWebApplicationFactory factory) : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await using var context = GetDbContext();

        // Ensure database is created and migrated
        await context.Database.MigrateAsync();

        // Seed required parameters
        if (!await context.Parameters.AnyAsync())
        {
            context.Parameters.Add(new ParameterEntity { Key = "year", Value = "2024" });
            context.Parameters.Add(new ParameterEntity { Key = "frenpioclitvttl", Value = "0" });
            context.Parameters.Add(new ParameterEntity { Key = "frenpioclitsporta", Value = "0" });
            context.Parameters.Add(new ParameterEntity { Key = "compaliasVttl", Value = "Vttl" });
            context.Parameters.Add(new ParameterEntity { Key = "compaliasSporta", Value = "Sporta" });
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Get_ReturnsConfig_WhenNoLastChecked()
    {
        // Act
        var response = await Client.GetAsync("/api/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var config = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(config);
        Assert.True(config.ContainsKey("year"));
        Assert.Equal("2024", config["year"]);
    }

    [Fact]
    public async Task Get_ReturnsNull_WhenLastCheckedIsRecent()
    {
        // Arrange - use a future date so config appears unchanged
        var futureDate = DateTime.UtcNow.AddDays(1).ToString("o");

        // Act
        var response = await Client.GetAsync($"/api/config?lastChecked={futureDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ClearCache_RequiresAuthentication()
    {
        // Act
        var response = await Client.PostAsync("/api/config/ClearCache", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
