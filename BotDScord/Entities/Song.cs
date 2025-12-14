using System.Collections.Immutable;

namespace BotDScord.Entities;

public sealed class Song
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;

    public string Url { get; init; } = null!;

    // in seconds
    public int Duration { get; init; }
    public MusicPlatform Platform { get; init; }
    public ImmutableList<PlaylistSong> PlaylistSongs { get; init; } = ImmutableList<PlaylistSong>.Empty;
}