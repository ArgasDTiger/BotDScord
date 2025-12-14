using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Gateway.Voice;

namespace BotDScord.Features.Voice.Services;

public sealed class MusicPlayer : IMusicPlayer
{
    private readonly ISongQueuer _songQueuer;
    private readonly IAudioStreamer _audioStreamer;
    private readonly GatewayClient _client;
    private readonly ILogger<MusicPlayer> _logger;

    public MusicPlayer(ISongQueuer songQueuer, IAudioStreamer audioStreamer, GatewayClient client,
        ILogger<MusicPlayer> logger)
    {
        _songQueuer = songQueuer;
        _audioStreamer = audioStreamer;
        _client = client;
        _logger = logger;
    }

    public async ValueTask PlayQueue(Guild guild, VoiceClient voiceClient)
    {
        try
        {
            while (_songQueuer.TryDequeueSong(guild.Id, out var song))
            {
                await using var outStream = voiceClient.CreateOutputStream();
                await using var opusStream = new OpusEncodeStream(outStream, PcmFormat.Short, VoiceChannels.Stereo,
                    OpusApplication.Audio);

                await _audioStreamer.StreamByUrl(song!.StreamInfo.Url, opusStream);
                await opusStream.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while playing the queue.");
        }
        finally
        {
            _songQueuer.RemoveQueueOfGuild(guild.Id);
            await voiceClient.CloseAsync();
            await _client.UpdateVoiceStateAsync(new VoiceStateProperties(guild.Id, null));
        }
    }
}