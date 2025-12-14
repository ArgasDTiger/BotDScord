using BotDScord.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BotDScord.Infrastructure.EntityConfigurations;

public sealed class PlaylistSongConfiguration : IEntityTypeConfiguration<PlaylistSong>
{
    public void Configure(EntityTypeBuilder<PlaylistSong> builder)
    {
        builder.ToTable(nameof(PlaylistSong));
        builder.HasKey(ps => new { ps.PlaylistId, ps.SongId });

        builder.HasOne(ps => ps.Playlist)
            .WithMany(p => p.PlaylistSongs)
            .HasForeignKey(ps => ps.PlaylistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Song)
            .WithMany(s => s.PlaylistSongs)
            .HasForeignKey(ps => ps.SongId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ps => ps.CreatedAtUtc)
            .IsRequired();
    }
}