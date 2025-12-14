using NetCord.Gateway;
using NetCord.Gateway.Voice;

namespace BotDScord.Features.Voice.Services;

public interface IMusicPlayer
{
    ValueTask PlayQueue(Guild guild, VoiceClient voiceClient);
}