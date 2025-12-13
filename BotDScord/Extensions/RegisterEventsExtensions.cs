using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;

namespace BotDScord.Extensions;

public static class RegisterEventsExtensions
{
    public static void RegisterEvents(this IHost host)
    {
        var client = host.Services.GetRequiredService<GatewayClient>();
        var voiceService = host.Services.GetRequiredService<IVoiceClientHandler>();

        client.VoiceStateUpdate += voiceState =>
            voiceState.UserId == client.Id ? voiceService.OnVoiceStateUpdate(voiceState) : ValueTask.CompletedTask;
    }
}