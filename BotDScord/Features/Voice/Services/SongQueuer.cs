using System.Collections.Concurrent;
using BotDScord.Features.Voice.Models;
using Microsoft.Extensions.Logging;

namespace BotDScord.Features.Voice.Services;

public sealed class SongQueuer : ISongQueuer
{
    private readonly ILogger<SongQueuer> _logger;
    private static readonly ConcurrentDictionary<ulong, ConcurrentQueue<SongQueueInfo>> Songs = new();

    public SongQueuer(ILogger<SongQueuer> logger)
    {
        _logger = logger;
    }

    public void QueueSong(ulong guildId, SongQueueInfo song)
    {
        var queue = GetQueue(guildId);
        queue.Enqueue(song);
    }

    public bool TryDequeueSong(ulong guildId, out SongQueueInfo? song)
    {
        var queue = GetQueue(guildId);
        return queue.TryDequeue(out song);
    }

    public void QueueSongs(ulong guildId, IEnumerable<SongQueueInfo> songs)
    {
        var queue = GetQueue(guildId);
        foreach (var song in songs)
        {
            queue.Enqueue(song);
        }
    }

    public void RemoveQueueOfGuild(ulong guildId) => Songs.TryRemove(guildId, out _);

    private static ConcurrentQueue<SongQueueInfo> GetQueue(ulong guildId) =>
        Songs.GetOrAdd(guildId, new ConcurrentQueue<SongQueueInfo>());
}