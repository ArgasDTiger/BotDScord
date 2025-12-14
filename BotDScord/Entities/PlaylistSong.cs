using BotDScord.Entities.Abstractions;

namespace BotDScord.Entities;

public sealed class PlaylistSong : ICreatable
{
    public Guid PlaylistId { get; init; }
    public Playlist Playlist { get; init; } = null!;
    public Guid SongId { get; init; }
    public Song Song { get; init; } = null!;
    public DateTimeOffset CreatedAtUtc { get; init; }
}