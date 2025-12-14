using NetCord.Gateway.Voice;

namespace BotDScord.Features.Voice.Services;

public interface IAudioStreamer
{
    Task StreamByUrl(string url, OpusEncodeStream opusStream);
}