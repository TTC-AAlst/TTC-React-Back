using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;
using Ttc.Model.Clubs;

namespace Ttc.UnitTests.Integration;

public class ClubsControllerTests : IntegrationTestBase
{
    public ClubsControllerTests(TtcWebApplicationFactory factory) : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await using var context = GetDbContext();

        // Ensure database is created and migrated
        await context.Database.MigrateAsync();

        // Seed required data
        if (!await context.Parameters.AnyAsync())
        {
            context.Parameters.Add(new ParameterEntity { Key = "year", Value = "2024" });
            await context.SaveChangesAsync();
        }

        if (!await context.Clubs.AnyAsync())
        {
            context.Clubs.Add(new ClubEntity
            {
                Id = 1,
                Name = "TTC Aalst",
                CodeVttl = "OVL135",
                CodeSporta = "4046",
                Active = true
            });
            context.Clubs.Add(new ClubEntity
            {
                Id = 2,
                Name = "TTC Dendermonde",
                CodeVttl = "OVL140",
                Active = true
            });
            context.Clubs.Add(new ClubEntity
            {
                Id = 3,
                Name = "Inactive Club",
                Active = false
            });
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Get_ReturnsActiveClubs()
    {
        // Act
        var response = await Client.GetAsync("/api/clubs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CacheResponse<Club>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Data);

        // Should only return active clubs
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, c => c.Name == "TTC Aalst");
        Assert.Contains(result.Data, c => c.Name == "TTC Dendermonde");
        Assert.DoesNotContain(result.Data, c => c.Name == "Inactive Club");
    }

    [Fact]
    public async Task Get_ReturnsNull_WhenNoChanges()
    {
        // Arrange - use a future date
        var futureDate = DateTime.UtcNow.AddDays(1).ToString("o");

        // Act
        var response = await Client.GetAsync($"/api/clubs?lastChecked={futureDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClub_RequiresAuthentication()
    {
        // Arrange
        var club = new Club { Id = 1, Name = "Updated Name" };

        // Act
        var response = await PostAsync("/api/clubs/UpdateClub", club);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Sync_RequiresAuthentication()
    {
        // Act
        var response = await Client.PostAsync("/api/clubs/Sync", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
