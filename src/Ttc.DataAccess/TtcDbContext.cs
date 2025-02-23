using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess;

internal class TtcDbContext : DbContext, ITtcDbContext
{
    private static int? _currentSeason;
    private readonly IUserProvider _userProvider;

    #region DbSets
    public DbSet<PlayerEntity> Players { get; set; }
    public DbSet<PlayerLoginEntity> PlayerLogins { get; set; }
    public DbSet<PlayerPasswordResetEntity> PlayerPasswordResets { get; set; }

    public DbSet<ClubEntity> Clubs { get; set; }
    public DbSet<ClubLocationEntity> ClubLocations { get; set; }
    public DbSet<ClubManagerEntity> ClubManagers { get; set; }

    public DbSet<TeamEntity> Teams { get; set; }
    public DbSet<TeamOpponentEntity> TeamOpponents { get; set; }
    public DbSet<TeamPlayerEntity> TeamPlayers { get; set; }
    public DbSet<MatchEntity> Matches { get; set; }
    public DbSet<MatchPlayerEntity> MatchPlayers { get; set; }
    public DbSet<MatchGameEntity> MatchGames { get; set; }
    public DbSet<MatchCommentEntity> MatchComments { get; set; }

    public DbSet<ParameterEntity> Parameters { get; set; }
    public DbSet<EventEntity> Events { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// The year of the current season.
    /// For season 2019-2020 this is 2019.
    /// </summary>
    public int CurrentSeason
    {
        get
        {
            if (_currentSeason.HasValue)
                return _currentSeason.Value;

            var year = Parameters.Single(x => x.Key == "year").Value;
            _currentSeason = int.Parse(year);
            return _currentSeason.Value;
        }
        set => _currentSeason = value;
    }

    public int CurrentFrenoySeason => CurrentSeason - 2000 + 1;
    #endregion

    public TtcDbContext(DbContextOptions<TtcDbContext> options, IUserProvider userProvider) : base(options)
    {
        _userProvider = userProvider;
    }

    /// <summary>
    /// Get the same time as the Frenoy Api
    /// </summary>
    public static DateTime GetCurrentBelgianDateTime()
    {
        DateTime belgianTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Romance Standard Time");
        return belgianTime;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClubLocationEntity>()
            .HasOne(c => c.Club)
            .WithMany(l => l.Locations)
            .HasForeignKey(x => x.ClubId)
            .IsRequired();

        modelBuilder.Entity<ClubManagerEntity>()
            .HasOne(c => c.Club)
            .WithMany(c => c.Managers)
            .HasForeignKey(x => x.ClubId)
            .IsRequired();

        modelBuilder.Entity<TeamEntity>()
            .Property(o => o.Competition)
            .HasConversion<string>();

        modelBuilder
            .Entity<EventEntity>()
            .Property(e => e.Type)
            .HasMaxLength(50)
            .HasConversion<string>();


        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => typeof(IAudit).IsAssignableFrom(entityType.ClrType))
            .ToArray();

        foreach (var entityType in entities)
        {
            modelBuilder.Entity(entityType.ClrType)
                .OwnsOne(typeof(Audit), nameof(IAudit.Audit), audit =>
                {
                    audit.Property(nameof(Audit.CreatedOn)).HasColumnName(nameof(Audit.CreatedOn));
                    audit.Property(nameof(Audit.CreatedBy)).HasColumnName(nameof(Audit.CreatedBy));
                    audit.Property(nameof(Audit.ModifiedOn)).HasColumnName(nameof(Audit.ModifiedOn));
                    audit.Property(nameof(Audit.ModifiedBy)).HasColumnName(nameof(Audit.ModifiedBy));
                });
        }
    }

    #region Audit
    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker
            .Entries<IAudit>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            Audit audit = entityEntry.Entity.Audit;
            if (entityEntry.State == EntityState.Added)
            {
                audit.CreatedOn = DateTime.Now;
                audit.CreatedBy = _userProvider.Name;
            }
            else
            {
                audit.ModifiedOn = DateTime.Now;
                audit.ModifiedBy = _userProvider.Name;
            }
        }
    }
    #endregion
}

/// <summary>
/// For EF Migrations
/// </summary>
internal class TtcDbContextFactory : IDesignTimeDbContextFactory<TtcDbContext>
{
    public TtcDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<TtcDbContext>();
        GlobalBackendConfiguration.ConfigureDbContextBuilder(builder);
        return new TtcDbContext(builder.Options, new MigrationsUserProvider());
    }
}
