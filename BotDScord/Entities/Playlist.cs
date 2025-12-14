using System.Collections.Immutable;
using BotDScord.Entities.Abstractions;

namespace BotDScord.Entities;

public sealed class Playlist : ICreatable, IDeletable
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? DeletedAtUtc { get; init; }
    public ulong UserId { get; init; }
    public User User { get; init; } = null!;
    public ImmutableList<PlaylistSong> PlaylistSongs { get; init; } = ImmutableList<PlaylistSong>.Empty;
}