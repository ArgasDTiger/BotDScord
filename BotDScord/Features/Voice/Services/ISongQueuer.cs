using BotDScord.Features.Voice.Models;

namespace BotDScord.Features.Voice.Services;

public interface ISongQueuer
{
    void QueueSong(ulong guildId, SongQueueInfo song);
    bool TryDequeueSong(ulong guildId, out SongQueueInfo? song);
    void QueueSongs(ulong guildId, IEnumerable<SongQueueInfo> songs);
    void RemoveQueueOfGuild(ulong guildId);
}