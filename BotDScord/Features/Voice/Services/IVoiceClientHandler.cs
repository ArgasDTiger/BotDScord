using NetCord.Gateway;
using NetCord.Gateway.Voice;

namespace BotDScord.Features.Voice.Services;

public interface IVoiceClientHandler
{
    ValueTask<OneOf<VoiceClient, Error>> GetVoiceClient(GatewayClient client, Guild guild, ulong userId);

    ValueTask OnVoiceStateUpdate(VoiceState state);
}