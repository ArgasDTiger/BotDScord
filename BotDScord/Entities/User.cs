using System.Collections.Immutable;

namespace BotDScord.Entities;

public sealed class User
{
    public ulong Id { get; init; }
    public ImmutableList<Playlist> Playlists { get; init; } = ImmutableList<Playlist>.Empty;
}