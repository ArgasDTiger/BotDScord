using System.Collections.Concurrent;
using BotDScord.Features.Voice.Models;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Gateway.Voice;
using NetCord.Logging;

namespace BotDScord.Features.Voice.Services;

public sealed class VoiceClientHandler : IVoiceClientHandler
{
    private readonly ILogger<VoiceClientHandler> _logger;
    private readonly ISongQueuer _songQueuer;
    private readonly ConcurrentDictionary<ulong, VoiceClient> _voiceClients = new();

    public VoiceClientHandler(ILogger<VoiceClientHandler> logger, ISongQueuer songQueuer)
    {
        _logger = logger;
        _songQueuer = songQueuer;
    }

    public async ValueTask<OneOf<VoiceClientResult, Error>> GetVoiceClient(GatewayClient client, Guild guild,
        ulong userId)
    {
        if (_voiceClients.TryGetValue(guild.Id, out var existingClient))
        {
            if (guild.VoiceStates.ContainsKey(client.Id))
            {
                return new VoiceClientResult(existingClient, IsNewClient: false);
            }

            _voiceClients.TryRemove(guild.Id, out _);
            existingClient.Dispose();
        }

        if (!guild.VoiceStates.TryGetValue(userId, out var userVoiceState))
        {
            return new Error("User is not connected to any voice channel.");
        }

        var voiceClient = await client.JoinVoiceChannelAsync(
            guild.Id,
            userVoiceState.ChannelId.GetValueOrDefault(),
            new VoiceClientConfiguration
            {
                Logger = new ConsoleLogger(),
            });

        await voiceClient.StartAsync();
        await voiceClient.EnterSpeakingStateAsync(new SpeakingProperties(SpeakingFlags.Microphone));

        if (!_voiceClients.TryAdd(guild.Id, voiceClient))
        {
            await voiceClient.CloseAsync();
            throw new VoiceClientException("Failed to cache voice client.");
        }

        return new VoiceClientResult(voiceClient, IsNewClient: true);
    }

    public ValueTask OnVoiceStateUpdate(VoiceState state)
    {
        if (state.ChannelId is null && _voiceClients.TryRemove(state.GuildId, out var removedClient))
        {
            _logger.LogDebug("Bot has disconnected from Guild {GuildId}.", state.GuildId);
            removedClient.Dispose();
        }

        _songQueuer.RemoveQueueOfGuild(state.GuildId);

        return ValueTask.CompletedTask;
    }
}