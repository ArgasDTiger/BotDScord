using System.Reflection;
using BotDScord.Entities;
using BotDScord.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BotDScord.Infrastructure;

public sealed class BotDScordDbContext : DbContext
{
    private string ConnectionString { get; }

    public BotDScordDbContext(DbContextOptions<BotDScordDbContext> options, IConfiguration configuration) :
        base(options)
    {
        ConnectionString = configuration.GetConnectionString("DefaultConnection") ??
                           throw new ArgumentNullException(nameof(configuration));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(ConnectionString, options =>
        {
            options.MapEnum<MusicPlatform>(nameof(MusicPlatform));
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<MusicPlatform>(name: nameof(MusicPlatform));
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistConfiguration ());
        modelBuilder.ApplyConfiguration(new PlaylistSongConfiguration());
        modelBuilder.ApplyConfiguration(new SongConfiguration());
    }
}