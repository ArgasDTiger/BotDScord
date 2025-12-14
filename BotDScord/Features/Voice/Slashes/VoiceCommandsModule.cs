using System.Diagnostics;
using BotDScord.Features.Voice.Models;
using BotDScord.Features.Voice.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BotDScord.Features.Voice.Slashes;

public sealed class VoiceCommandsModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<VoiceCommandsModule> _logger;
    private readonly IVoiceClientHandler _voiceClientHandler; 
    private readonly ISongQueuer _songQueuer;
    private readonly IMusicPlayer _musicPlayer;

    public VoiceCommandsModule(
        ILogger<VoiceCommandsModule> logger,
        IVoiceClientHandler voiceClientHandler,
        ISongQueuer songQueuer,
        IMusicPlayer musicPlayer)
    {
        _logger = logger;
        _voiceClientHandler = voiceClientHandler;
        _songQueuer = songQueuer;
        _musicPlayer = musicPlayer;
    }

    [SlashCommand("play", "Plays music from YouTube", Contexts = [InteractionContextType.Guild])]
    public async Task PlayAsync(string youtubeUrl)
    {
        var guild = Context.Guild;
        var client = Context.Client;
        var user = Context.User;

        if (guild is null)
        {
            await RespondAsync(InteractionCallback.Message("This command can only be used on the server!"));
            return;
        }

        if (!guild.VoiceStates.TryGetValue(user.Id, out var voiceState))
        {
            await RespondAsync(InteractionCallback.Message("You are not connected to any voice channel!"));
            return;
        }

        bool isBotInVoice = guild.VoiceStates.TryGetValue(client.Id, out var botVoiceState);

        if (isBotInVoice && botVoiceState!.ChannelId != voiceState.ChannelId)
        {
            await RespondAsync(InteractionCallback.Message("I am already connected to another voice channel!"));
            return;
        }

        await RespondAsync(InteractionCallback.DeferredMessage());

        var voiceClientResult = await _voiceClientHandler.GetVoiceClient(client, guild, user.Id);

        if (voiceClientResult.TryPickT1(out var error, out var voiceClient))
        {
            await RespondAsync(InteractionCallback.Message(error.Message));
            return;
        }

        // TODO: handle exception
        var youtubeResult = await GetYoutubeAudioStream(youtubeUrl);

        if (youtubeResult.TryPickT1(out var youtubeError, out var songInfo))
        {
            await RespondAsync(InteractionCallback.Message(youtubeError.Message));
            return;
        }

        var songQueueInfo = youtubeResult.AsT0!;

        _songQueuer.QueueSong(guild.Id, songQueueInfo);
        await FollowupAsync(new InteractionMessageProperties
            { Content = $"Queued {songInfo.Video.Title} by {songInfo.Video.Author}" });

        if (voiceClient.IsNewClient)
        {
            _ = _musicPlayer.PlayQueue(guild, voiceClient.VoiceClient);
        }
    }

    private async Task<OneOf<SongQueueInfo, Error>> GetYoutubeAudioStream(string youtubeUrl)
    {
        try
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(youtubeUrl);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(youtubeUrl);
            IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            return new SongQueueInfo(streamInfo, video);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[YoutubeExplode] Error: {Message}", ex.Message);
            return new Error("Failed to get audio from YouTube.");
        }
    }
}