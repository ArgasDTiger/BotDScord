using BotDScord.Features.Voice.Models;
using NetCord.Gateway;

namespace BotDScord.Features.Voice.Services;

public interface IVoiceClientHandler
{
    ValueTask<OneOf<VoiceClientResult, Error>> GetVoiceClient(GatewayClient client, Guild guild, ulong userId);

    ValueTask OnVoiceStateUpdate(VoiceState state);
}