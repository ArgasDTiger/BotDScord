using BotDScord.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BotDScord.Infrastructure.EntityConfigurations;

public sealed class SongConfiguration : IEntityTypeConfiguration<Song>
{
    public void Configure(EntityTypeBuilder<Song> builder)
    {
        builder.ToTable(nameof(Song));
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        builder.Property(s => s.Name).IsRequired().HasMaxLength(255);
        builder.Property(s => s.Url).IsRequired().HasMaxLength(2000);
        builder.Property(s => s.Duration).IsRequired();
        builder.Property(s => s.Platform).IsRequired();
    }
}